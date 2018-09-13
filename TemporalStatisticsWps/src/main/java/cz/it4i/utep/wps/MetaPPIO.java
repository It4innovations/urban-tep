package cz.it4i.utep.wps;

import java.io.InputStream;

import org.geoserver.wps.ppio.XMLPPIO;
import org.xml.sax.ContentHandler;
import org.xml.sax.helpers.AttributesImpl;

import javax.xml.namespace.QName;

public class MetaPPIO extends XMLPPIO {
	
	public MetaPPIO() {
		super(MetaArrayList.class, MetaArrayList.class, "application/xml", new QName("result"));
	}
	
	@Override
    public void encode(Object object, ContentHandler handler) throws Exception {
        handler.startDocument();
        MetaArrayList list = (MetaArrayList) object;
        encode((MetaArrayList) object, handler);
        handler.endDocument();
    }
	
	void encode(MetaArrayList<?> list, ContentHandler h) throws Exception {
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
