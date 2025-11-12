using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Xrm.CrmSdk;
#else
using Source.DLaB.Xrm.CrmSdk;
#endif

namespace DLaB.Xrm.LocalCrm
{
    internal class CrmExceptions
    {
        /// <summary>
        /// Error Codes for Invalid ConditionExpression Values
        /// </summary>
        public const int InvalidConditionValue = -2146233087;

        public static FaultException<OrganizationServiceFault> GetAttributeCanNotBeSpecifiedIfAnAggregateOperationIsRequestedException()
        {
            return CreateFault(ErrorCodes.InvalidOperation, "Attribute can not be specified if an aggregate operation is requested.");
        }
        public static FaultException<OrganizationServiceFault> GetEntityIdMustBeTheSameException()
        {
            return CreateFault(ErrorCodes.UnExpected, "Entity Id must be the same as the value set in property bag.");
        }

        public static FaultException<OrganizationServiceFault> GetEntityDoesNotExistException(Entity entity)
        {
            return CreateFault(ErrorCodes.ObjectDoesNotExist, $"{ErrorCodes.GetErrorMessage(ErrorCodes.ObjectDoesNotExist)}  {entity.LogicalName} With Id = {entity.Id} Does Not Exist");
        }

        public static FaultException<OrganizationServiceFault> GetInvalidAttributeTypeException(Type type)
        {
            return CreateFault(ErrorCodes.UnExpected, $"Incorrect type of attribute value {type.FullName}");
        }

        public static FaultException<OrganizationServiceFault> GetInvalidCharacterSpecifiedForAlias(string aliasName)
        {
            return CreateFault(ErrorCodes.UnExpected, $"Invalid character specified for alias: {aliasName}. Only characters within the ranges [A-Z], [a-z] or [0-9] or _ are allowed.  The first character may only be in the ranges [A-Z], [a-z] or _.");
        }

        public static FaultException<OrganizationServiceFault> GetConditionValueGreaterThan0Exception()
        {
            return CreateFault(InvalidConditionValue, "The condition value should be greater than zero.");
        }

        public static FaultException<OrganizationServiceFault> GetConditionOperatorRequiresValuesException(ConditionExpression condition, int requiredValues)
        {
            return CreateFault(InvalidConditionValue, requiredValues == 0
                ? $"Condition operator '{condition.Operator}' requires that no values are set. Values.Length: {condition.Values.Count}"
                : $"The ConditionOperator.{condition.Operator} requires {requiredValues} value/s, not {condition.Values.Count}. Parameter Name: {condition.AttributeName}.");
        }

        public static FaultException<OrganizationServiceFault> GetIntShouldBeStringOrIntException(string qualifiedAttributeName)
        {
            return CreateFault(InvalidConditionValue, $"Condition for attribute '{qualifiedAttributeName}': integer values are expected to be passed as strings or int.");
        }

        public static FaultException<OrganizationServiceFault> GetDateShouldBeStringOrDateException(string qualifiedAttributeName)
        {
            return CreateFault(InvalidConditionValue, $"The date-time format for {qualifiedAttributeName} is invalid, or value is outside the supported range.");
        }

        public static FaultException<OrganizationServiceFault> GetFormatterException(Type type)
        {
            return CreateFault(-1, string.Format(@"The formatter threw an exception while trying to deserialize the message: There was an error while trying to deserialize parameter http://schemas.microsoft.com/xrm/2011/Contracts/Services:query. The InnerException message was 'Error in line 1 position 1978. Element 'http://schemas.microsoft.com/2003/10/Serialization/Arrays:anyType' contains data from a type that maps to the name " +
                "'{0}:{1}'.The deserializer has no knowledge of any type that maps to this name. Consider changing the implementation of the ResolveName method on your DataContractResolver to return a non-null value for name '{1}' and namespace '{0}'.'. Please see InnerException for more details.", type.Namespace, type.Name));
        }

        public static FaultException<OrganizationServiceFault> GetTopCountCantBeSpecifiedWithPagingInfoException(int topCount)
        {
            return CreateFault(InvalidConditionValue, $"The Top.Count = {topCount} can't be specified with pagingInfo");
        }

        public static FaultException<OrganizationServiceFault> GetFaultException(int hResult, params object[] args)
        {
            var message = ErrorCodes.GetErrorMessage(hResult);
            if (args.Length > 0)
            {
                message = string.Format(message, args);
            }

            return CreateFault(hResult, message);
        }

        private static FaultException<OrganizationServiceFault> CreateFault(int errorCode, string message)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = errorCode,
                Message = message,
                Timestamp = DateTime.UtcNow
#if NET
            }, new FaultReason(message))
            {
                HResult = errorCode
            };
#else                
            }, message);
#endif
        }
    }
}
