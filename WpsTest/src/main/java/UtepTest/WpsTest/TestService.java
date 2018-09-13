package UtepTest.WpsTest;

import org.geotools.process.factory.DescribeParameter;
import org.geotools.process.factory.DescribeProcess;
import org.geotools.process.factory.DescribeResult;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.io.Console;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.lang.annotation.*;
import java.net.MalformedURLException;
import java.net.URL;

import org.geoserver.wps.gs.GeoServerProcess;
import org.geoserver.wps.process.*;

import org.apache.velocity.Template;
import org.apache.velocity.VelocityContext;
import org.apache.velocity.app.VelocityEngine;
import org.apache.velocity.runtime.RuntimeConstants;
import org.apache.velocity.runtime.resource.loader.ClasspathResourceLoader;

@DescribeProcess(title="BulkProcessingService", description="Test bulk processing service example.")
public class TestService implements GeoServerProcess {

   @DescribeResult(name="Result", description="Processing result", meta = {"mimeTypes=text/xml"})
   public String execute(
		   @DescribeParameter(name="inputFolderPath", description="Path to the input data folder.", min = 1, max = 1, meta = { "mimeTypes=text/plain" }) final String folderPath
		   //@DescribeParameter(name="stringValue", description="name to return", min = 1, max = 1, meta = { "mimeTypes=text/plain" }) final String stringValue,
		   //@DescribeParameter(name="inputDataSet", description="enum parameter", min = 1, max = 1) final EDataSet inputDataSet,
		   //@DescribeParameter(name = "outputMimeType", min = 0,	max = 1) final String outputMimeType
	   ) throws MalformedURLException {
	   
	   return "Processing started, file path: " + folderPath;
	   
	   /**
	   String inputEnumString = "";
	   try {
	    	Annotation[] annotations = EDataSet.class.getField(inputDataSet.name()).getAnnotations();
	 	   	for (Annotation annotation : annotations) {
	 	   		if (annotation instanceof EnumStringValue ) {
					EnumStringValue myAnnotation = (EnumStringValue) annotation;
					inputEnumString += myAnnotation.value();
				}
	 		}
   		} catch (Exception e) {
   			inputEnumString += e.getMessage();
		}
	   
	   inputEnumString += " " + stringValue;
	   
	   ArrayList<String> outputs = new ArrayList<String>();
	   outputs.add(new URL("http", "SERVERURL", "/results/" + "result1.png").toString());
	   outputs.add(new URL("http", "SERVERURL", "/results/" + "result2.png").toString());
	   outputs.add(new URL("http", "SERVERURL", "/results/" + "metadata.xml").toString());
	   
	   String executeResponse = "";
	   if(outputMimeType.endsWith("application/atom+xml")){
		   executeResponse = CreateAtomResponse(outputs);
	   }
	   else if(outputMimeType.equals("text/xml")) {
		   executeResponse = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urls><url>"+inputEnumString+"</url></urls>";
	   }
	   else if(outputMimeType.equals("application/json")) {
		   executeResponse = "{\"data\":" + "\"" + inputEnumString + "\"}";
	   }
	   else {
		   executeResponse = inputEnumString;
	   }
	   
       return new StringRawData(executeResponse, outputMimeType);
       */
	   
	   
   }
   
   /**
   public String CreateAtomResponse(ArrayList<String> outputs){
	   
	   //String meta = CreateMetadataFile();
	   
	   String atomResponse = "";
	   atomResponse += "<atomOutputs>";
	   for (String output : outputs) {
		   atomResponse += "<output>" + output + "</output>";
	   }
	   atomResponse += "</atomOutputs>";
	   return atomResponse;
   }
   
   
   public String CreateMetadataFile(){
	   
	   //test data
	   String productionId = "productionId";
	   String processTitle = "processTitle";
	   String processName = "processName";
	   String jobFinishTime = "jobFinishTime";
	   String collectionUrl = "collectionUrl";
	   
	   ArrayList<HashMap<String, String>> productList = new ArrayList<HashMap<String, String>>();
	   HashMap<String, String> h = new HashMap<String, String>();
	   h.put("productUrl", "url1");
	   h.put("productFileName", "name1");
	   h.put("productMimeType", "image/png");
	   h.put("productFileSize", "10");
	   productList.add(h);
	   h = new HashMap<String, String>();
	   h.put("productUrl", "url2");
	   h.put("productFileName", "name2");
	   h.put("productMimeType", "image/png");
	   h.put("productFileSize", "20");
	   productList.add(h);
	   
	   String regionWkt =  "regionWkt";
	   String startDate = "startDate";
	   String stopDate = "stopDate";
	   String regionBox = "regionBox";
	   ArrayList<String> quickLookProductUrlList = new ArrayList<String>();
	   quickLookProductUrlList.add("qurl1");
	   quickLookProductUrlList.add("qurl2");
	   String outputFormat = "outputFormat";
	   String productionType = "productionType";
	   String processorName = "processorName";
	   String processorVersion = "processorVersion";
	   
	   VelocityEngine ve = new VelocityEngine();
	   ve.setProperty(RuntimeConstants.RESOURCE_LOADER, "classpath");
	   ve.setProperty("classpath.resource.loader.class", ClasspathResourceLoader.class.getName());
	   ve.init();
       
       Template vt = ve.getTemplate("templates/metadata.vm");
	   
       VelocityContext context = new VelocityContext();
       context.put("productionId", productionId);
       context.put("processTitle", processTitle);
       context.put("processName", processName);
       context.put("jobFinishTime", jobFinishTime);
       context.put("collectionUrl", collectionUrl);
       context.put("productList", productList);
       context.put("regionWkt", regionWkt);
       context.put("startDate", startDate);
       context.put("stopDate", stopDate);
       context.put("regionBox", regionBox);
       
       context.put("quickLookProductUrlList", quickLookProductUrlList);
       context.put("outputFormat", outputFormat);
       context.put("productionType", productionType);
       context.put("processorName", processorName);
       context.put("processorVersion", processorVersion);
       
       StringWriter writer = new StringWriter();
       vt.merge(context, writer );
       
       try{
    	   PrintWriter pwriter = new PrintWriter("./src/main/java/templates/metadata.xml", "UTF-8");
    	   pwriter.print(writer.toString());
    	   pwriter.close();
       } catch (IOException e) {
    	   System.out.println(e.getMessage());
    	}
       
       return writer.toString();
   }
   */
}
