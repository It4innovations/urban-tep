package cz.it4i.utep.wps;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.lang.annotation.Annotation;
import java.net.MalformedURLException;
import java.net.URL;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

import javax.net.ssl.HttpsURLConnection;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.geoserver.wps.gs.GeoServerProcess;
import org.geotools.geometry.jts.ReferencedEnvelope;
import org.geotools.process.factory.DescribeParameter;
import org.geotools.process.factory.DescribeProcess;
import org.geotools.process.factory.DescribeResult;
import org.geotools.process.factory.DescribeResults;
import org.geotools.text.Text;
import org.geotools.util.NullProgressListener;
import org.opengis.util.ProgressListener;

import org.apache.commons.io.FilenameUtils;
import org.apache.commons.lang.StringUtils;
import org.apache.velocity.Template;
import org.apache.velocity.VelocityContext;
import org.apache.velocity.app.VelocityEngine;
import org.apache.velocity.runtime.RuntimeConstants;
import org.apache.velocity.runtime.resource.loader.ClasspathResourceLoader;

import cz.it4i.haas.JobStateExt;
import cz.it4i.haas.SubmittedJobInfoExt;
import cz.it4i.haas.SubmittedJobInfoUsageReportExt;
import cz.it4i.haas.adapter.HpcAsAServiceAdapter;

import cz.it4i.utep.wps.JsonBuilder.JsonBuilder;

@DescribeProcess(title = "TemporalStatistics", description = "Temporal Statistics")
public class TemporalStatistics implements GeoServerProcess{
	private static final long COMMAND_TEMPLATE_ID = 1;
	private static final long CLUSTER_NODE_TYPE_ID = 1;
	
	private static final int NUM_OF_CORES = 24;

	private static final int DEFAULT_WALLTIME = 3600;

	private static String HPCAAS_USERNAME;
	private static String HPCAAS_PASSWORD;
	private static String REMOTE_REF;
	
	private static final String POST_USERNAME_CAT = "";
	private static final String POST_PASSWORD_CAT = "";
	
	private static final String POST_USERNAME_ACC = "";
	private static final String POST_PASSWORD_ACC = "";
	
	private HpcAsAServiceAdapter adapter = new HpcAsAServiceAdapter();
	
	private static final String PATH_FOR_DOWNLOADED_RESULTS = "G:\\results";
	
	@DescribeResults({
		@DescribeResult(name = "result_metadata", description = "OpenSearch Description to the Results", type=MetaArrayList.class, meta = {"mimeTypes=text/xml"}),
		@DescribeResult(name = "QUOTATION", description = "Quotation offering", type=OfferArrayList.class, meta = {"mimeTypes=text/xml"})
	})
	public Map<String, Object> execute(
			@DescribeParameter(name = "productionName", description = "The name of the product", defaultValue = "TemporalStatistics", min = 1, max = 1) String productionName,
			@DescribeParameter(name = "maxCloudCover", description = "The maximum acceptable cloud cover per input image, [0,100]", defaultValue = "25", min = 1, max = 1) float maxCloudCover,
			@DescribeParameter(name = "inputDataSet", description = "The input dataset required for the processing", min = 0, max = 1) EDataSet inputDataSet,
			@DescribeParameter(name = "minDate", description = "The desired start date of the products in the format YYYY-MM-DD. Example: 2015-01-01", min = 1, max = 1) Date minDate,
			@DescribeParameter(name = "maxDate", description = "The desired end date of the products in the format YYYY-MM-DD. Example: 2015-03-01", min = 1, max = 1) Date maxDate,
			@DescribeParameter(name = "regionWkt", description = "The spatial range in the format of text. Example: POLYGON((100 -10,100 0,110 0,110 -10,100 -10))", min = 0, max = 1) String regionWkt,
			@DescribeParameter(name = "regionBB", description = "The spatial range in the format of bounding box.", min = 0, max = 1) ReferencedEnvelope regionBB,
			@DescribeParameter(name = "options", description = "Optional parameters.", min = 0) final String options,
			@DescribeParameter(name = "quotation", description = "Quotation parameter.", min = 0) final Boolean quotation,
			ProgressListener monitor
			) throws InterruptedException, IOException, DatatypeConfigurationException, ParseException {
		
		if(regionWkt == null && regionBB == null)
			throw new IOException("regionWkt or regionBB was not provided");
		
		Map<String, Object> result = new HashMap<String, Object>();
		
		/**
		System.out.println("productionName: " + productionName);
		System.out.println("maxCloudCover: " + maxCloudCover);
		System.out.println("inputDataSet: " + inputDataSet);
		System.out.println("minDate: " + minDate);
		System.out.println("maxDate: " + maxDate);
		System.out.println("regionWkt: " + regionWkt);
		System.out.println("regionBB: " + regionBB);
		System.out.println("options: " + options);
		*/
		
		if( monitor == null ) monitor = new NullProgressListener();
		monitor.started();
		
		//outputs
		MetaArrayList<String> outputResults = new MetaArrayList<String>();
		OfferArrayList<String> offerResult = new OfferArrayList<String>();
		
		//input params
		Map<String, String> parameters = fillApplicationSpecificParameterMap(maxCloudCover, inputDataSet, minDate, maxDate, regionWkt, regionBB);
		
		//parse user credentials from options
		if(options != null && options != ""){
			String[] pairs = options.split(",");
			for (String pair: pairs){
				String[] keyValue = pair.split(":");
				if(keyValue.length == 2){
					if (keyValue[0].trim().equals("userName"))
						HPCAAS_USERNAME = keyValue[1].trim();
					else if (keyValue[0].trim().equals("userPasswd"))
						HPCAAS_PASSWORD = keyValue[1].trim();
					else if (keyValue[0].trim().equals("remoteRef"))
						REMOTE_REF = keyValue[1].trim().replace("_", "-");
				}
			}
		}else outputResults.add("User authentication failed.");
		
		///**
		//quotation check
		if(quotation == false)
		{
			///**
			//create and submit new job
			if(HPCAAS_USERNAME != null && HPCAAS_PASSWORD != null) 
			{		
				//create and submit job
				String sessionCode = adapter.authenticateUser(HPCAAS_USERNAME, HPCAAS_PASSWORD);
				SubmittedJobInfoExt job = adapter.createJob(sessionCode, productionName, COMMAND_TEMPLATE_ID, CLUSTER_NODE_TYPE_ID, NUM_OF_CORES, DEFAULT_WALLTIME, parameters);
				job = adapter.submitJob(sessionCode, job.getId());
				
				monitor.setTask( Text.text("Job status: " + JobStateExt.Configuring.toString()) );
				monitor.progress(10.0f);
				
				//state check
				do {
					synchronized (job) {
						job.wait(30000);
						job = adapter.getCurrentJobInfo(sessionCode, job.getId());
						System.out.println(job.getId() + " " + job.getState());
						
						monitor.setTask( Text.text("Job status: " + job.getState().toString()) );
						monitor.progress(50.0f);
					}
				} while ( job.getState().equals(JobStateExt.Configuring) || job.getState().equals(JobStateExt.Submitted) || job.getState().equals(JobStateExt.Queued) || job.getState().equals(JobStateExt.Running) );
				
				if(job.getState().equals(JobStateExt.Finished))
				{
					//productTitle used for accounting
					String productionTitle = "";
					
					//download results
					monitor.setTask( Text.text("Downloading results." ));
					monitor.progress(90.0f);
					String[] downloadedFiles = adapter.downloadResultFilesFromCluster(sessionCode, job.getId(), PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/" + job.getId());
					
					//publish results
					monitor.setTask( Text.text("Publishing results." ));
					monitor.progress(95.0f);
					URL[] resultUrls = publishDownloadedFiles(job.getId(), downloadedFiles);
					
					//include only metadata file
					//for (URL result: resultUrls){
					//	outputResults.add(result.toString());
					//}
					
					//generate metadata file
					ArrayList<HashMap<String, String>> productList = new ArrayList<HashMap<String, String>>();
					ArrayList<String> quickLookProductUrlList = new ArrayList<String>();
					HashMap<String, String> h;
					for (int i = 0; i < downloadedFiles.length; i++) 
					{	
						String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/" + job.getId() + "/" + downloadedFiles[i];
						
						if(FilenameUtils.getExtension(path).equals("tif") || FilenameUtils.getExtension(path).equals("tiff") || FilenameUtils.getExtension(path).equals("png"))
						{
							h = new HashMap<String, String>();
							URL url = new URL("https", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/" + job.getId() + downloadedFiles[i]);
							h.put("productUrl", url.toString());
							h.put("productFileName", downloadedFiles[i].replace("/Output/", ""));
							//h.put("productTitle", productionName + "_" + downloadedFiles[i].replace("/Output/", ""));
							//h.put("productId", job.getId() + "_" + downloadedFiles[i].replace("/Output/", ""));
							h.put("productTitle", job.getId() + "_" + productionName + "_" + downloadedFiles[i].replace("/Output/", ""));
							h.put("productId", job.getId() + "_" + productionName + "_" + downloadedFiles[i].replace("/Output/", ""));
							
							String mimeType = FilenameUtils.getExtension(path);
							if(mimeType.equals("png"))
								mimeType = "image/png";
							else if(mimeType.equals("tif"))
								mimeType = "image/tiff";
							h.put("productMimeType", mimeType);
							
							File f = new File(path);
							Long size = f.length();
							h.put("productFileSize", size.toString());
							//productList.add(h);
							
							if(mimeType.equals("image/png") || mimeType.equals("png"))
							{
								if(quickLookProductUrlList.size() == 0)
								{
									if(path.contains("1_11_8"))
										quickLookProductUrlList.add(url.toString());
								}
							}
							else{
								productList.add(h);
								productionTitle = job.getId() + "_" + productionName + "_" + downloadedFiles[i].replace("/Output/", "");
							}
						}
					}
					URL collectionUrl = new URL("https", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/" + job.getId());
					URL metaUrl = CreateMetadataFile(
						job.getId(),
						productionName,
						"TimeScan Landsat 2015",
						job.getEndTime(),
						collectionUrl,
						regionWkt,
						minDate,
						maxDate,
						regionBB,
						quickLookProductUrlList,
						productList,
						"NetCDF4",
						job.getNodeType().getName(),
						"TimeScan Service",
						"1.0"
					);
					
					//add metadata to outputs
					outputResults.add(metaUrl.toString());
					
					//build json resource report
					String json = BuildJson(job.getId(), productionTitle);
					
					monitor.setTask( Text.text("Job finished." ));
					
				}else {
					monitor.setTask( Text.text("Job failed." ));
				}
			}
			
		}
		else if (quotation == true)
		{
			//generate quotation offer
			outputResults.add("");
			String offer = CreateQuotationOffer();
			offerResult.add(offer);
		}
		//*/
		
		/**
		////////////////////////////////////////////////////////
		//JUST FOR DEBUG
		
		if(HPCAAS_USERNAME != null && HPCAAS_PASSWORD != null) 
		{
			//generate metadata file
			ArrayList<HashMap<String, String>> productList = new ArrayList<HashMap<String, String>>();
			HashMap<String, String> h;
			URL collectionUrl = new URL("http", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/11");
			Calendar calendar = Calendar.getInstance();
			URL metaUrl = CreateMetadataFile(
				(long)11,
				productionName,
				"TimeScan Landsat 2015",
				calendar,
				collectionUrl,
				regionWkt,
				minDate,
				maxDate,
				regionBB,
				null,
				productList,
				"NetCDF4",
				"Job TestName",
				"TimeScan Service",
				"1.0"
			);
			
			//add metadata to outputs
			outputResults.add(metaUrl.toString());
			
			//build json resource report
			//String json = BuildJson((long) 11);
			//System.out.println("Json generated.");
			
			//HaaS test
			//HpcAsAServiceAdapter adapter = new HpcAsAServiceAdapter();
			//String sessionCode = adapter.authenticateUser(HPCAAS_USERNAME, HPCAAS_PASSWORD);
			//System.out.println(sessionCode);
		}
		
		monitor.setTask( Text.text("Job finished." ));
		////////////////////////////////////////////////////////
		*/
		
		monitor.complete();
		
		//added for quota
		result.put("result_metadata", outputResults);
		result.put("QUOTATION", offerResult);
		
		return result;
	}
	
	private String BuildJson(Long jobId, String productionTitle) throws DatatypeConfigurationException, ParseException, IOException{
		
		//TODO query HaaS for resourceusage, parse parameters for json
		String sessionCode = adapter.authenticateUser(HPCAAS_USERNAME, HPCAAS_PASSWORD);
		SubmittedJobInfoUsageReportExt usageReport = adapter.getResourceUsageReportForJob(jobId, sessionCode);
		
		String timeStamp = "";
		if(usageReport.getEndTime() != null)
		{			
			GregorianCalendar c = new GregorianCalendar();
			c.setTime(usageReport.getEndTime().getTime());
			XMLGregorianCalendar cal = DatatypeFactory.newInstance().newXMLGregorianCalendar(c);
			timeStamp = cal.toString();
		}
		
		JsonBuilder jsonBuilder = new JsonBuilder();
		
		
		Map<String, Long> dict = new HashMap<String, Long>();
		
		String params = usageReport.getAllParameters();
		String[] lines = params.split("\n");
		ArrayList<String> selLines = new ArrayList<String>();
		for (String line: lines){
			if(line.contains("resources_used.cput:") || line.contains("resources_used.mem:") || line.contains("resources_used.ncpus:"))
				selLines.add(line);
		}
		for (String line: selLines){
			String[] keyValue = line.split(": ");
			if(keyValue[0].contains("cput")){
				String[] splitted = keyValue[1].split(":");
				long hours = Long.parseLong(splitted[0]) * 60 * 60 * 1000;
				long minutes = Long.parseLong(splitted[1]) * 60 * 1000;
				long secs = Long.parseLong(splitted[2]) * 1000;
				dict.put("CPU_MILLISECONDS", hours + minutes + secs);
			}else if (keyValue[0].contains("mem")){
				String memValue = keyValue[1].replaceAll("kb", "");
				dict.put("PHYSICAL_MEMORY_BYTES", Long.parseLong(memValue) * 1000);
			}else if (keyValue[0].contains("ncpus")){
				dict.put("CPU_COUNT", Long.parseLong(keyValue[1]));
			}
			
		}
		//add totalAllocatedTime, totalCorehours
		double altime = usageReport.getTotalAllocatedTime();
		double coreh = usageReport.getTotalCorehoursUsage();
		coreh = Math.ceil(coreh);
		dict.put("totalAllocatedTime", (long) altime);
		dict.put("totalCoreHoursUsage", (long) coreh);
		
        double[] coords = new double[] { 18.2820, 49.8346 };
        String json = jsonBuilder.CreateProcessingStatisticsJSON(
            usageReport.getId().toString(),
        	//productionTitle,
            "urban-tep",
            HPCAAS_USERNAME,
            REMOTE_REF,
            usageReport.getId().toString(),
            "Temporal Statistics WPS",
            "WPS",
            usageReport.getAllParameters(),
            dict,
            "SERVERURL",
            timeStamp,
            "WPS processing",
            coords);
        
        try{
	    	String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/"+ jobId +"/";
	    	File f = new File(path);
	    	f.mkdirs();
	    	PrintWriter pwriter = new PrintWriter(path + "resources.json", "UTF-8");
	    	pwriter.print(json.toString());
	    	pwriter.close();
	    } catch (IOException e) {
	    	System.out.println(e.getMessage());
	    }
        
        //POST request with json to accounting
        Map<String, String> headers = new HashMap<String, String>();
	    headers.put("Content-Type", "application/json");
	    SendPostRequest(jobId, "POST", POST_USERNAME_ACC, POST_PASSWORD_ACC, "T2URL", headers, json.toString(), "POST_resources_response.txt");
        
        return json;
	}
	
	//private URL CreateQuotationOffer() throws DatatypeConfigurationException, MalformedURLException{
	private String CreateQuotationOffer() throws DatatypeConfigurationException, MalformedURLException{
		JsonBuilder jsonBuilder = new JsonBuilder();
		
		Map<String, Long> dict = new HashMap<String, Long>();
		
		long hours = 5;
		long memValueMB = 120000;
		long cpuCount = 24;
		dict.put("CPU_MILLISECONDS", hours * 60 * 60 * 1000);
		dict.put("PHYSICAL_MEMORY_BYTES", memValueMB * 1000 * 1000);
		dict.put("CPU_COUNT", cpuCount);
		
		//add totalAllocatedTime, totalCorehours
		double altime = 10000;
		double coreh = 10;
		coreh = Math.ceil(coreh);
		dict.put("totalAllocatedTime", (long) altime);
		dict.put("totalCoreHoursUsage", (long) coreh);
		
		String timeStamp = "";
		GregorianCalendar cNow = (GregorianCalendar) GregorianCalendar.getInstance();
		XMLGregorianCalendar cal = DatatypeFactory.newInstance().newXMLGregorianCalendar(cNow);
		timeStamp = cal.toString();
		
        double[] coords = new double[] { 18.2820, 49.8346 };
        String json = jsonBuilder.CreateProcessingStatisticsJSON(
            "quotationOffer",
            "urban-tep",
            HPCAAS_USERNAME,
            REMOTE_REF,
            "quotationOffer",
            "Temporal Statistics WPS",
            "WPS",
            "",
            dict,
            "SERVERURL",
            timeStamp,
            "WPS processing",
            coords);
        
        //json = "<![CDATA[" + json + "]]>";
        UUID offerId = UUID.randomUUID();
        
        try{
	    	String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/quotationOffers/"+ offerId +"/";
	    	File f = new File(path);
	    	f.mkdirs();
	    	PrintWriter pwriter = new PrintWriter(path + "quotation.json", "UTF-8");
	    	pwriter.print(json.toString());
	    	pwriter.close();
	    } catch (IOException e) {
	    	System.out.println(e.getMessage());
	    }
        
        //URL url = new URL("https", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/quotationOffers/" + offerId + "/" + "quotation.json");
	    //return url;
        
        return json;
	}
	
	public URL CreateMetadataFile(
			Long jobId,
			String processTitle,
			String processName,
			Calendar jobFinishTime,
			URL collectionUrl,
			String regionWkt,
			Date startDate,
			Date stopDate,
			ReferencedEnvelope regionBox,
			ArrayList<String> quickLookProductUrlList,
			ArrayList<HashMap<String, String>> productList,
			String outputFormat,
			String productionType,
			String processorName,
			String processorVersion
		) throws InterruptedException, IOException, DatatypeConfigurationException {
		
		SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd");
		
		VelocityEngine ve = new VelocityEngine();
		ve.setProperty(RuntimeConstants.RESOURCE_LOADER, "classpath");
		ve.setProperty("classpath.resource.loader.class", ClasspathResourceLoader.class.getName());
		ve.init();
		
	    Template vt = ve.getTemplate("templates/metadata2.vm");
	       
	    VelocityContext context = new VelocityContext();
	    context.put("productionId", jobId.toString());
	    context.put("processTitle", processTitle);
	    context.put("processName", processName);
	    
	    String TjobFinishTime = "";
		if(jobFinishTime != null)
		{			
			GregorianCalendar c = new GregorianCalendar();
			c.setTime(jobFinishTime.getTime());
			XMLGregorianCalendar cal = DatatypeFactory.newInstance().newXMLGregorianCalendar(c);
			TjobFinishTime = cal.toString();
		}
	    context.put("jobFinishTime", TjobFinishTime);
	    String TcollectionUrl = collectionUrl == null ? "" : collectionUrl.toString();
	    context.put("collectionUrl", TcollectionUrl);
	    context.put("productList", productList);
	    
	    String TregionWkt = "";
	    int regionWktPoints = 0;
	    if(regionWkt != null)
	    {	
	    	regionWktPoints = StringUtils.countMatches(regionWkt, ",") + 1;
	    	String temp = regionWkt.toString();
	    	temp = temp.replace("POLYGON ((", "");
	    	temp = temp.replace("POLYGON((", "");
	    	temp = temp.replace("))", "");
	    	temp = temp.replaceAll(",", " ");
	    	TregionWkt = temp;
	    	
	    	String YXregionWKT = "";
	    	String[] coordinates = TregionWkt.split(" ");
	    	ArrayList<Double> xValues = new ArrayList<Double>();
	    	ArrayList<Double> yValues = new ArrayList<Double>();
	    	for(int i = 0; i < coordinates.length; i = i + 2){
	    		xValues.add(Double.parseDouble(coordinates[i]));
	    		yValues.add(Double.parseDouble(coordinates[i+1]));
	    	}
	    	for(int i = 0; i < xValues.size(); i = i + 1){
	    		if(YXregionWKT.equals(""))
	    			YXregionWKT = yValues.get(i) + " " + xValues.get(i);
	    		else YXregionWKT += " " + yValues.get(i) + " " + xValues.get(i);
	    	}
	    	TregionWkt = YXregionWKT;
	    }else if(regionBox != null){
	    	regionWktPoints = 5;
	    	TregionWkt = regionBox.getMinY() + " " + regionBox.getMinX() + " "
	    			+ regionBox.getMinY() + " " + regionBox.getMaxX() + " "
	    			+ regionBox.getMaxY() + " " + regionBox.getMaxX() + " "
					+ regionBox.getMaxY() + " " + regionBox.getMinX() + " "
	    			+ regionBox.getMinY() + " " + regionBox.getMinX();
	    }
	    context.put("regionWktPoints", regionWktPoints);
	    context.put("regionWkt", TregionWkt);
	    
	    //String TstartDate = startDate == null ? "" : sdf.format(startDate);
	    String TstartDate = "";
		if(startDate != null)
		{			
			GregorianCalendar c = new GregorianCalendar();
			c.setTime(startDate);
			XMLGregorianCalendar cal = DatatypeFactory.newInstance().newXMLGregorianCalendar(c);
			TstartDate = cal.toString();
		}
	    context.put("startDate", TstartDate);
	    
	    //String TstopDate = stopDate == null ? "" : sdf.format(stopDate);
	    String TstopDate = "";
		if(stopDate != null)
		{			
			GregorianCalendar c = new GregorianCalendar();
			c.setTime(stopDate);
			XMLGregorianCalendar cal = DatatypeFactory.newInstance().newXMLGregorianCalendar(c);
			TstopDate = cal.toString();
		}
	    context.put("stopDate", TstopDate);
	    
	    String TregionBox = "";
	    if(regionBox != null)
	    {	
	    	//String temp = regionBox.toString();
	    	//temp = temp.replaceAll(",", " ");
	    	String temp = regionBox.getMinY() + " " + regionBox.getMinX() + " " + regionBox.getMaxY() + " " + regionBox.getMaxX();
	    	TregionBox = temp;
	    }else if (TregionWkt != ""){
	    	String[] coordinates = TregionWkt.split(" ");
	    	ArrayList<Double> xValues = new ArrayList<Double>();
	    	ArrayList<Double> yValues = new ArrayList<Double>();
	    	for(int i = 0; i < coordinates.length; i = i + 2){
	    		xValues.add(Double.parseDouble(coordinates[i]));
	    		yValues.add(Double.parseDouble(coordinates[i+1]));
	    	}
	    	TregionBox = Collections.min(xValues).toString() + " " + Collections.min(yValues).toString()
	    			 + " " + Collections.max(xValues).toString() + " " + Collections.max(yValues).toString();
	    }
	    context.put("regionBox", TregionBox);
	      
	    context.put("quickLookProductUrlList", quickLookProductUrlList);
	    context.put("outputFormat", outputFormat);
	    context.put("productionType", productionType);
	    context.put("processorName", processorName);
	    context.put("processorVersion", processorVersion);
	    
	    StringWriter writer = new StringWriter();
	    vt.merge(context, writer );
	    
	    try{
	    	String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/" + jobId + "/";
	    	File f = new File(path);
	    	f.mkdirs();
	    	PrintWriter pwriter = new PrintWriter(path + "metadata.xml", "UTF-8");
	    	pwriter.print(writer.toString());
	    	pwriter.close();
	    } catch (IOException e) {
	    	System.out.println(e.getMessage());
	    }
	    
	    //POST request with metadata to catalog
	    Map<String, String> headers = new HashMap<String, String>();
	    headers.put("Content-Type", "application/atom+xml");
	    SendPostRequest(jobId, "POST", POST_USERNAME_CAT, POST_PASSWORD_CAT, "T2URL", headers, writer.toString(), "POST_metadata_response.txt");
	    
	    URL metaUrl = new URL("https", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/" + jobId + "/" + "metadata.xml");
	    return metaUrl;
	}
	
	private void SendPostRequest(long jobId, String method, String login, String password, String url, Map<String, String> headers, String body, String responseFile) throws IOException{
		try
		{
			URL obj = new URL(url);
			HttpsURLConnection con = (HttpsURLConnection) obj.openConnection();
			con.setRequestMethod(method);
			
			//add reuqest header
			for (String key: headers.keySet()){
				con.setRequestProperty(key, headers.get(key));
			}
			
			String loginPassword = login+ ":" + password;
			String encoded = new sun.misc.BASE64Encoder().encode (loginPassword.getBytes());
			con.setRequestProperty ("Authorization", "Basic " + encoded);
	
			// write body
			if (body != null) {
				con.setDoOutput(true);
				DataOutputStream wr = new DataOutputStream(con.getOutputStream());
				wr.writeBytes(body);
				wr.flush();
				wr.close();
			}
			
			int responseCode = con.getResponseCode();
			System.out.println("\nSending '" + method + "' request to URL : " + url);
			System.out.println("Response Code : " + responseCode);
	
			BufferedReader in = new BufferedReader(new InputStreamReader(con.getInputStream()));
			StringBuffer response = new StringBuffer();
			String inputLine;
			while ((inputLine = in.readLine()) != null) {
				response.append(inputLine);
			}
			in.close();
	
			//print result
			//System.out.println(response.toString());
			try{
		    	String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/" + jobId + "/";
		    	File f = new File(path);
		    	f.mkdirs();
		    	PrintWriter pwriter = new PrintWriter(path + responseFile, "UTF-8");
		    	pwriter.print(response.toString());
		    	pwriter.close();
		    } catch (IOException e) {
		    	System.out.println(e.getMessage());
		    }
		} catch (Exception e){
			System.out.println("POST request failed.");
			//System.out.println(e.getMessage());
			try{
		    	String path = PATH_FOR_DOWNLOADED_RESULTS + "/" + HPCAAS_USERNAME + "/" + jobId + "/";
		    	File f = new File(path);
		    	f.mkdirs();
		    	PrintWriter pwriter = new PrintWriter(path + responseFile, "UTF-8");
		    	pwriter.print(e.getMessage());
		    	pwriter.close();
		    } catch (IOException ee) {
		    	System.out.println(ee.getMessage());
		    }
		}
	}
	
	/**
	 * @param downloadedFiles
	 * @return
	 * @throws MalformedURLException 
	 */
	private URL[] publishDownloadedFiles(long jobId, String[] downloadedFiles) throws MalformedURLException {
		URL[] result = new URL[downloadedFiles.length];
		for (int i = 0; i < downloadedFiles.length; i++) {
			URL url = new URL("https", "SERVERURL", "/results/" + HPCAAS_USERNAME + "/" + jobId + downloadedFiles[i]);
			result[i] = url;
		}
		return result;
	}

	/**
	 * @param maxCloudCover
	 * @param inputDataSet
	 * @param minDate
	 * @param maxDate
	 * @param regionWkt
	 * @param regionBB
	 * @return
	 */
	private Map fillApplicationSpecificParameterMap(float maxCloudCover, EDataSet inputDataSet, Date minDate, Date maxDate, String regionWkt, ReferencedEnvelope regionBB) {
		Map result = new HashMap<String, String>();
		result.put("maxCloudCover", formatFloat(maxCloudCover));
		result.put("inputDataSet", getProcessorRepresentationOfDataSet(inputDataSet));
		DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd");
		result.put("minDate", dateFormat.format(minDate));
		result.put("maxDate", dateFormat.format(maxDate));
		String region = null;
		if ( regionBB != null ) {
			region = "POLYGON((" + regionBB.getMinX() + " " + regionBB.getMinY() + ", " + regionBB.getMinX() + " " + regionBB.getMaxY() + ", " + regionBB.getMaxX() + " " + regionBB.getMaxY() + ", "
					+ regionBB.getMaxX() + " " + regionBB.getMinY() + ", " + regionBB.getMinX() + " " + regionBB.getMinY() + "))";
		} else {
			region = regionWkt;
		}
		result.put("region", region);
		return result;
	}

	/**
	 * @param inputDataSet
	 * @return
	 */
	private String getProcessorRepresentationOfDataSet(EDataSet inputDataSet){
		try {
	    	Annotation[] annotations = EDataSet.class.getField(inputDataSet.name()).getAnnotations();
	    	for (Annotation annotation : annotations) {
	 	   		if (annotation instanceof EnumStringValue ) {
					EnumStringValue myAnnotation = (EnumStringValue) annotation;
					return myAnnotation.value();
				}
	 		}
   		} catch (Exception e) {
   			System.out.println(e.getMessage());
		}
		return inputDataSet.toString();
	}
	/*private String getProcessorRepresentationOfDataSet(EDataSet inputDataSet) {
		return "Landsat 8 Level 1";
	}
	*/
	/*private String getProcessorRepresentationOfDataSet(EDataSet inputDataSet) {
		try {
			Annotation[] annotations = EDataSet.class.getField(inputDataSet.name()).getAnnotations();
			for (Annotation annotation : annotations) {
				if ( annotation instanceof EnumStringValue ) {
					EnumStringValue myAnnotation = (EnumStringValue) annotation;
					return myAnnotation.value();
				}
			}
		} catch (NoSuchFieldException e) {
			e.printStackTrace();
		} catch (SecurityException e) {
			e.printStackTrace();
		}
		return null;
	}
	*/
	
	public static String formatFloat(float d) {
		if ( d == (long) d )
			return String.format("%d", (long) d);
		else
			return String.format("%s", d);
	}
}