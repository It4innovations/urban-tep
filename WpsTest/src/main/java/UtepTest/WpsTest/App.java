package UtepTest.WpsTest;

import java.lang.annotation.Annotation;
import java.util.ArrayList;

import org.geoserver.wps.process.RawData;
import org.geoserver.wps.process.StringRawData;

import UtepTest.WpsTest.EDataSet;

import java.io.IOException;
import java.lang.annotation.*;

/**
 * Hello world!
 *
 */
public class App 
{
    public static void main( String[] args ) throws InterruptedException, IOException
    {
    	/**
    	ArrayList<Product> productList = new ArrayList<Product>();
 	   	productList.add(new Product("url1", "name1", "image/png", 10));
 	   	productList.add(new Product("url2", "name2", "image/png", 20));
    	
 	   	for (Product prod : productList){
 	   		System.out.println(prod.productFileName);
 	   	}
 	   	*/
    	
    	/**
    	TestService testServ = new TestService();
    	RawData rawdata = testServ.execute(
    			"testString",
    			EDataSet.Landsat_8_Level_1,
    			"application/atom+xml"
    			);
    	StringRawData srd = (StringRawData)rawdata;
		System.out.println(srd.getData());
		*/
    	
    	TestService testServ = new TestService();
    	String response = testServ.execute("C:\\inetpub\\wwwroot\\HaasPublished");
    	System.out.println(response);
    }   
}
