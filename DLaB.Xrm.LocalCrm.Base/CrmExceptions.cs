using System;
using System.ServiceModel;
using DLaB.Xrm.CrmSdk;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal class CrmExceptions
    {
        /// <summary>
        /// Error Codes for Invalid ConditionExpression Values
        /// </summary>
        public const int InvalidConditionValue = -2146233087;

        public static FaultException<OrganizationServiceFault> GetEntityDoesNotExistException(Entity entity)
        {
            var message = $"{ErrorCodes.GetErrorMessage(ErrorCodes.ObjectDoesNotExist)}  {entity.LogicalName} With Id = {entity.Id} Does Not Exist";
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = ErrorCodes.ObjectDoesNotExist,
                Message = message,
                Timestamp = DateTime.UtcNow,
            }, message);
        }

        public static FaultException<OrganizationServiceFault> GetConditionValueGreaterThan0Exception()
        {
            var message = $"The condition value should be greater than zero.";
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = InvalidConditionValue,
                Message = message,
                Timestamp = DateTime.UtcNow,
            }, message);
        }

        public static FaultException<OrganizationServiceFault> GetIntShouldBeStringOrIntException(string qualifiedAttributeName)
        {
            var message = $"Condition for attribute '{qualifiedAttributeName}': integer values are expected to be passed as strings or int.";
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = InvalidConditionValue,
                Message = message,
                Timestamp = DateTime.UtcNow,
            }, message);
        }

        public static FaultException<OrganizationServiceFault> GetFaultException(int hResult, params object[] args)
        {
            var message = ErrorCodes.GetErrorMessage(hResult);
            if (args.Length > 0)
            {
                message = string.Format(message, args);
            }
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = hResult,
                Message = message,
                Timestamp = DateTime.UtcNow
            }, message);
        }
    }
}
