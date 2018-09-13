package cz.it4i.utep.wps;

import java.io.InputStream;
import java.util.ArrayList;

import org.geoserver.wps.ppio.XMLPPIO;
import org.geotools.xml.schema.Attribute;
import org.xml.sax.Attributes;
import org.xml.sax.ContentHandler;
import org.xml.sax.helpers.AttributesImpl;

import javax.xml.namespace.QName;

public class ListUrlPPIO extends XMLPPIO {
	
	public ListUrlPPIO() {
		super(ArrayList.class, ArrayList.class, "application/xml", new QName("result"));
	}
	
	@Override
    public void encode(Object object, ContentHandler handler) throws Exception {
        handler.startDocument();
        ArrayList list = (ArrayList) object;
        encode((ArrayList) object, handler);
        handler.endDocument();
    }
	
	void encode(ArrayList<?> list, ContentHandler h) throws Exception {
        for (Object entry : list) {
        	AttributesImpl atts = new AttributesImpl();
        	atts.addAttribute("", "href", "href", "CDATA", entry.toString());
        	atts.addAttribute("", "method", "method", "CDATA", "GET");
        	atts.addAttribute("", "mimeType", "mimeType", "CDATA", "application/atom+xml");
        	h.startElement(null, "wps:Reference", "wps:Reference", atts);
            h.endElement(null, "wps:Reference", "wps:Reference");
        }
    }
	
	 @Override
	 public Object decode(InputStream input) throws Exception {
		 throw new UnsupportedOperationException();
	 }
}
