package cz.it4i.utep.wps;

import java.io.InputStream;

import org.geoserver.wps.ppio.XMLPPIO;
import org.xml.sax.ContentHandler;
import org.xml.sax.helpers.AttributesImpl;

import javax.xml.namespace.QName;

public class OfferPPIO extends XMLPPIO {
	
	public OfferPPIO() {
		super(OfferArrayList.class, OfferArrayList.class, "application/json", new QName("offer"));
	}
	
	@Override
    public void encode(Object object, ContentHandler handler) throws Exception {
        handler.startDocument();
        OfferArrayList list = (OfferArrayList) object;
        encode((OfferArrayList) object, handler);
        handler.endDocument();
    }
	
	void encode(OfferArrayList<?> list, ContentHandler h) throws Exception {
        for (Object entry : list) {
        	h.characters(((String)entry).toCharArray(), 0, ((String)entry).toCharArray().length);
        }
    }
	
	 @Override
	 public Object decode(InputStream input) throws Exception {
		 throw new UnsupportedOperationException();
	 }
}
