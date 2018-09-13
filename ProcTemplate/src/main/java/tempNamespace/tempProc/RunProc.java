package tempNamespace.tempProc;

import java.io.IOException;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Map;

import javax.xml.datatype.DatatypeConfigurationException;

import org.apache.commons.cli.CommandLine;
import org.apache.commons.cli.CommandLineParser;
import org.apache.commons.cli.DefaultParser;
import org.apache.commons.cli.HelpFormatter;
import org.apache.commons.cli.Option;
import org.apache.commons.cli.Options;
import org.geotools.geometry.jts.ReferencedEnvelope;

public class RunProc {

	/**
	 * @param args
	 * @throws IOException 
	 * @throws InterruptedException 
	 * @throws DatatypeConfigurationException 
	 * @throws ParseException 
	 * @throws org.apache.commons.cli.ParseException 
	 */
	public static void main(String[] args) throws InterruptedException, IOException, DatatypeConfigurationException, ParseException {
		
		///**
		Options cliOptions = new Options();

        Option productionName = new Option("n", "productionName", true, "production name");
        productionName.setRequired(true);
        cliOptions.addOption(productionName);
        
        Option minDate = new Option("a", "minDate", true, "min date");
        minDate.setRequired(true);
        cliOptions.addOption(minDate);
        
        Option maxDate = new Option("z", "maxDate", true, "max date");
        maxDate.setRequired(true);
        cliOptions.addOption(maxDate);
        
        Option regionWkt = new Option("k", "regionWkt", true, "wkt region");
        regionWkt.setRequired(false);
        cliOptions.addOption(regionWkt);
        
        Option regionBB = new Option("b", "regionBB", true, "bb region");
        regionBB.setRequired(false);
        cliOptions.addOption(regionBB);
        
        Option options = new Option("o", "options", true, "options");
        options.setRequired(true);
        cliOptions.addOption(options);
        
        Option quotation = new Option("q", "quotation", true, "quotation");
        quotation.setRequired(true);
        cliOptions.addOption(quotation);
        
        Option workDir = new Option("w", "workDir", true, "working directory");
        workDir.setRequired(true);
        cliOptions.addOption(workDir);

        CommandLineParser parser = new DefaultParser();
        HelpFormatter formatter = new HelpFormatter();
        CommandLine cmd;
        
        try {
            cmd = parser.parse(cliOptions, args);
        } catch (org.apache.commons.cli.ParseException e) {
        	 System.out.println(e.getMessage());
             formatter.printHelp("utility-name", cliOptions);
             System.exit(1);
             return;
		}

      	String pProductionName = cmd.getOptionValue("productionName");
      	String pMinDate = cmd.getOptionValue("minDate");
      	String pMaxDate = cmd.getOptionValue("maxDate");
      	String pRegionWkt = cmd.getOptionValue("regionWkt");
      	String pRegionBB = cmd.getOptionValue("regionBB");
      	String pOptions = cmd.getOptionValue("options");
      	String temQuotation = cmd.getOptionValue("quotation");
      	String pWorkDir = cmd.getOptionValue("workDir");
        
      	Boolean pQuotation = false;
        if(temQuotation.toLowerCase().equals("true"))
        	pQuotation = true;
      	
		ProcTemplate proc = new ProcTemplate();
		Map<String, List<String>> mainResult = proc.execute(
				pProductionName, 
				pMinDate, 
				pMaxDate,
				pRegionWkt,
				pRegionBB,
				pOptions,
				pQuotation, 
				pWorkDir
			);
		
		//print outputs
	    List<String> outputRes = mainResult.get("result_metadata");
		for (String res: outputRes){
			System.out.println("result_metadata: " + res);
		}
		
		List<String> quotatioRes = mainResult.get("QUOTATION");
		for (String res: quotatioRes){
			System.out.println("QUOTATION: " + res);
		}
		
	}
	
}
