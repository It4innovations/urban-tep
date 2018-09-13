package cz.it4i.utep.wps;

import java.io.IOException;
import java.net.URL;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.GregorianCalendar;

import javax.xml.datatype.DatatypeConfigurationException;

import org.geoserver.wps.process.*;

public class HaaSWsTestConsole {

	/**
	 * @param args
	 * @throws IOException 
	 * @throws InterruptedException 
	 * @throws DatatypeConfigurationException 
	 * @throws ParseException 
	 */
	public static void main(String[] args) throws InterruptedException, IOException, DatatypeConfigurationException, ParseException {
		TemporalStatistics stat = new TemporalStatistics();
		
		/**
		RawData rawdata = stat.execute(
				"TestTemporalStat", 
				25f, 
				EDataSet.Landsat_8_Level_1, 
				new GregorianCalendar(2015,Calendar.JANUARY,1).getTime(), 
				new GregorianCalendar(2015,Calendar.DECEMBER,1).getTime(),
				"POLYGON ((-96.761475051669208 35.532897256625745,-96.687438784176777 35.523025754293599,-96.680857782621956 35.440763234857457,-96.723634292728661 35.450634737189773,-96.756539300503107 35.486830245741601,-96.761475051669208 35.532897256625745))", 
				null
				);
		
		StringRawData srd = (StringRawData)rawdata;
		System.out.println(srd.getData());
		
		*/
		
		/**
		ArrayList<String> list = stat.execute(
				"ConsoleTest", 
				25f, 
				EDataSet.Landsat_8_Level_1, 
				new GregorianCalendar(2015,Calendar.JANUARY,1).getTime(), 
				new GregorianCalendar(2015,Calendar.MARCH,1).getTime(),
				"POLYGON ((-96.761475051669208 35.532897256625745,-96.687438784176777 35.523025754293599,-96.680857782621956 35.440763234857457,-96.723634292728661 35.450634737189773,-96.756539300503107 35.486830245741601,-96.761475051669208 35.532897256625745))", 
				null,
				"userName: ; userPasswd: ;",
				null
				);
		
		for (String url: list){
			System.out.println(url);
		}
		*/
		
		/**
		URL metaUrl = stat.execute(
				"ConsoleTest", 
				25f, 
				EDataSet.Landsat_8_Level_1, 
				new GregorianCalendar(2015,Calendar.JANUARY,1).getTime(), 
				new GregorianCalendar(2015,Calendar.MARCH,1).getTime(),
				"POLYGON ((-96.761475051669208 35.532897256625745,-96.687438784176777 35.523025754293599,-96.680857782621956 35.440763234857457,-96.723634292728661 35.450634737189773,-96.756539300503107 35.486830245741601,-96.761475051669208 35.532897256625745))", 
				null,
				"userName: ; userPasswd: ;",
				null
				);
		System.out.println(metaUrl.toString());
		*/
	}

}
