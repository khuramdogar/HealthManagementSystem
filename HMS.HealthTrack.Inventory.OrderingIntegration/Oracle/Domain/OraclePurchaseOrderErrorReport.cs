using System;
using System.Collections.Generic;
using HMS.HealthTrack.Inventory.Common;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OraclePurchaseOrderErrorReport
    {
        private const int ExpectedFieldCount = 8;
        private const char InputDelimeter = '|';

        private readonly long _orderId;
        private readonly long _orderLineId;
        private readonly string _itemDescription;
        private readonly long _requestId;
        private readonly DateTime _creationDate;
        private readonly string _processFlag;
        private readonly string _interfaceSourceCode;

        public OraclePurchaseOrderErrorReport(string inputLine)
        {
            var splitData = inputLine.Split(InputDelimeter);

            if (splitData.Length != ExpectedFieldCount)
                throw new ArgumentException(string.Format("exception=Invalid number of fields in input, inputLine=\"{0}\", delimeter='{1}', expectedCount={2}, actualFieldCount={3}",
                    inputLine, InputDelimeter, ExpectedFieldCount, splitData.Length));

            _orderId = ConvertArgument<long>(splitData, OraclePurchaseOrderErrorReportField.FeederSystemRequisitionNumber);
            _orderLineId = ConvertArgument<long>(splitData, OraclePurchaseOrderErrorReportField.FeederSystemLineNumber);
            _itemDescription = splitData[(int)OraclePurchaseOrderErrorReportField.ItemDescription];
            _requestId = ConvertArgument<long>(splitData, OraclePurchaseOrderErrorReportField.RequestId);
            _creationDate = ConvertArgument<DateTime>(splitData, OraclePurchaseOrderErrorReportField.CreationDate);
            _processFlag = splitData[(int)OraclePurchaseOrderErrorReportField.ProcessFlag];
            _interfaceSourceCode = splitData[(int)OraclePurchaseOrderErrorReportField.InterfaceSourceCode];
        }

        /// <summary>
        /// Unique Requisition Number
        /// </summary>
        public long OrderId
        {
            get { return _orderId; }
        }

        /// <summary>
        /// Unique Requisition Line Number
        /// </summary>
        public long OrderLineId
        {
            get { return _orderLineId; }
        }

        /// <summary>
        /// Description of an Item
        /// </summary>
        public string ItemDescription
        {
            get { return _itemDescription; }
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId
        {
            get { return _requestId; }
        }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreationDate
        {
            get { return _creationDate; }
        }

        /// <summary>
        /// Interface source (eg :HTRAK) 
        /// </summary>
        public string InterfaceSourceCode
        {
            get { return _interfaceSourceCode; }
        }

        /// <summary>
        /// Status of the line
        /// </summary>
        public string ProcessFlag
        {
            get { return _processFlag; }
        }

        public override bool Equals(Object obj)
        {
            return obj is OraclePurchaseOrderErrorReport &&
                   this == (OraclePurchaseOrderErrorReport)obj;
        }

        public override int GetHashCode()
        {
            var hash = _orderId.GetHashCode() ^ 397;
            hash = hash ^ _orderLineId.GetHashCode();
            hash = hash ^ _itemDescription.GetHashCode();
            hash = hash ^ _requestId.GetHashCode();
            hash = hash ^ _creationDate.GetHashCode();
            hash = hash ^ _processFlag.GetHashCode();
            hash = hash ^ _interfaceSourceCode.GetHashCode();

            return hash;
        }

        public static bool operator ==(OraclePurchaseOrderErrorReport x, OraclePurchaseOrderErrorReport y)
        {
            return x.OrderId == y.OrderId &&
                   x.OrderLineId == y.OrderLineId &&
                   x.ItemDescription.Equals(y.ItemDescription) &&
                   x.RequestId == y.RequestId &&
                   x.CreationDate.Equals(y.CreationDate) &&
                   x.ProcessFlag.Equals(y.ProcessFlag) &&
                   x.InterfaceSourceCode.Equals(y.InterfaceSourceCode);
        }

        public static bool operator !=(OraclePurchaseOrderErrorReport x, OraclePurchaseOrderErrorReport y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Convert string to T or throw <see cref="ArgumentException"/>
        /// </summary>
        private static T ConvertArgument<T>(IList<string> splitData, OraclePurchaseOrderErrorReportField field)
        {
            return splitData[(int)field].ConvertArgument<T>(field.ToString());
        }
    }
}
