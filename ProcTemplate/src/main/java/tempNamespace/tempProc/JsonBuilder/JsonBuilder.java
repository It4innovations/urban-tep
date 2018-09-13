package tempNamespace.tempProc.JsonBuilder;

import java.util.ArrayList;
import java.util.Map;

import com.google.gson.Gson;

public class JsonBuilder {
	public String CreateProcessingStatisticsJSON(String id, String accountPlatform, String accountUserName, String accountRef, String compoundId, String compoundName, String compoundType, 
			String compoundAnyJobId, Map<String, Long> quantityDictionary, String hostName, String timeStamp, String status, double[] locationCoordinates)
        {
            ProcessingStatisticsDTO psDTO = new ProcessingStatisticsDTO();
            psDTO.id = id;
            
            psDTO.account = new Account();
            psDTO.account.platform = accountPlatform;
            psDTO.account.userName = accountUserName;
            psDTO.account.ref = accountRef;
            
            psDTO.compound = new Compound();
            psDTO.compound.id = compoundId;
            psDTO.compound.name = compoundName;
            psDTO.compound.type = compoundType;
            psDTO.compound.any = new Any();
            psDTO.compound.any.allParameters = compoundAnyJobId;
            
            psDTO.quantity = GetQuantityFromDictionary(quantityDictionary);
            psDTO.hostname = hostName;
            psDTO.timestamp = timeStamp;
            psDTO.status = status;
            
            psDTO.location = new Location();
            psDTO.location.coordinates = locationCoordinates;
            
            return GetJsonFromProcessingStatisticsDTO(psDTO);
        }
	
	private Quantity[] GetQuantityFromDictionary(Map<String, Long> quantityDictionary)
    {
		//ProcessingStatisticsDTO.Quantity[] quantities = new ProcessingStatisticsDTO.Quantity[quantityDictionary.size()];
		ArrayList<Quantity> quantities = new ArrayList<Quantity>();
        //for (int index = 0; index < quantityDictionary.size(); index++)
		for (String key: quantityDictionary.keySet())
		{
			Quantity q = new Quantity();
			q.id = key;
			q.value = quantityDictionary.get(key);
			quantities.add(q);
        }
		return quantities.toArray(new Quantity[quantities.size()]);
    }
	
	private String GetJsonFromProcessingStatisticsDTO(ProcessingStatisticsDTO psDTO)
    {
		Gson gson = new Gson();
		String json = gson.toJson(psDTO);
		return json;
    }
}
