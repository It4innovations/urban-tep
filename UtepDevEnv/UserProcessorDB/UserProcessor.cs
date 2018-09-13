using System;
using System.Collections.Generic;
using UserProcessorDB.Data;
using UserProcessorDB.DAO;

namespace UserProcessorDB
{
    public class UserProcessor
    {
        // Install-Package log4net

        UserProcessorDAO dao = new UserProcessorDAO();

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public long CreateNamespaceInfo()
        {
            Log.Debug("CreateNamespaceInfo()");

            try
            {
                return new NamespaceInfoDAO().CreateNamespaceInfo();
            }
            catch (Exception e)
            {
                Log.Error("Error in CreateNamespaceInfo: ", e);
                return -1;
            }
        }

        public UserProcessorDTO GetLastUserProcessor(string processorName)
        {
            Log.Debug(string.Format("ProcessorName: {0}", processorName));

            try
            {
                return dao.GetLastUserProcessor(processorName);
            }
            catch (Exception e)
            {
                Log.Error("Error in GetLastUserProcessor: ", e);
                return null;
            }
        }

        public List<UserProcessorDTO> GetAllUserProcessors(string userName)
        {
            Log.Debug(string.Format("UserName: {0}", userName));

            try
            {
                return dao.GetAllUserProcessors(userName);
            }
            catch (Exception e)
            {
                Log.Error("Error in GetAllUserProcessors: ", e);
                return null;
            }
        }

        public void InsertNewUserProcessor(UserProcessorDTO userProcessor)
        {
            Log.Debug(string.Format("UserName: {0}, ProcessorName: {1}, Version: {2}, CreationalTime: {3}, IsActive: {4}",
                userProcessor.UserName, userProcessor.ProcessorName, userProcessor.Version, userProcessor.CreationalTime, userProcessor.IsActive));

            try
            {
                dao.InsertNewUserProcessor(userProcessor);
            }
            catch (Exception e)
            {
                Log.Error("Error in InsertNewUserProcessor: ", e);
            }
        }

        public void UpdateUserProcessorActiveStatus(UserProcessorDTO userProcessor)
        {
            Log.Debug(string.Format("UserName: {0}, ProcessorName: {1}, Version: {2}, CreationalTime: {3}, IsActive: {4}",
                userProcessor.UserName, userProcessor.ProcessorName, userProcessor.Version, userProcessor.CreationalTime, userProcessor.IsActive));

            try
            {
                dao.UpdateUserProcessorActiveStatus(userProcessor);
            }
            catch (Exception e)
            {
                Log.Error("Error in UpdateUserProcessorActiveStatus: ", e);
            }
        }

        public void DeleteUserProcessor(UserProcessorDTO userProcessor)
        {
            Log.Debug(string.Format("UserName: {0}, ProcessorName: {1}, Version: {2}, CreationalTime: {3}, IsActive: {4}",
                userProcessor.UserName, userProcessor.ProcessorName, userProcessor.Version, userProcessor.CreationalTime, userProcessor.IsActive));

            try
            {
                dao.DeleteUserProcessor(userProcessor);
            }
            catch (Exception e)
            {
                Log.Error("Error in DeleteUserProcessor: ", e);
            }
        }
    }
}
