using System;
using System.Collections.Generic;
using HMS.HealthTrack.Inventory.Common;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OraclePurchaseOrderReceipt
    {
        private const int ExpectedFieldCount = 13;
        private const char InputDelimeter = '|';

        private readonly long _orderId;
        private readonly long _orderLineId;
        private readonly long _fmisRequisitionNumber;
        private readonly long _fmisRequisitionLineNumber;
        private readonly long _fmisRequisitionDistributionNumber;
        private readonly string _fmisDistribution;
        private readonly long _fmisVendorCode;
        private readonly string _fmisSupplierProductCode;
        private readonly string _fmisHospitalProductCode;
        private readonly double _fmisPricePerUnit;
        private readonly string _fmisUnitOfMeasure;
        private readonly string _fmisOrgId;

        public OraclePurchaseOrderReceipt(string inputLine)
        {
            var splitData = inputLine.Split(InputDelimeter);

            if (splitData.Length != ExpectedFieldCount)
                throw new ArgumentException(string.Format("exception=Invalid number of fields in input, inputLine=\"{0}\", delimeter='{1}', expectedCount={2}, actualFieldCount={3}",
                    inputLine, InputDelimeter, ExpectedFieldCount, splitData.Length));

            _orderId = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FeederSystemRequisitionNumber);
            _orderLineId = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FeederSystemLineNumber);
            _fmisRequisitionNumber = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FmisRequisitionNumber);
            _fmisRequisitionLineNumber = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FmisRequisitionLineNumber);
            _fmisRequisitionDistributionNumber = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FmisRequisitionDistributionNumber);
            _fmisDistribution = splitData[(int)OraclePurchaseOrderField.FmisDistribution];
            _fmisVendorCode = ConvertArgument<long>(splitData, OraclePurchaseOrderField.FmisVendorCode);
            _fmisSupplierProductCode = splitData[(int)OraclePurchaseOrderField.FmisSupplierProductCode];
            _fmisHospitalProductCode = splitData[(int)OraclePurchaseOrderField.FmisHospitalProductCode];
            _fmisPricePerUnit = ConvertArgument<double>(splitData, OraclePurchaseOrderField.FmisPricePerUnit);
            _fmisUnitOfMeasure = splitData[(int)OraclePurchaseOrderField.FmisUnitOfMeasure];
            _fmisOrgId = splitData[(int)OraclePurchaseOrderField.FmisOrgId];
        }

        /// <summary>
        /// Unique Requisition Number in Feeder System
        /// </summary>
        public long OrderId
        {
            get { return _orderId; }
        }

        /// <summary>
        /// Unique Requisition Line Number in Feeder System
        /// </summary>
        public long OrderLineId
        {
            get { return _orderLineId; }
        }

        /// <summary>
        /// FMIS Requisition Number
        /// </summary>
        public long FmisRequisitionNumber
        {
            get { return _fmisRequisitionNumber; }
        }

        /// <summary>
        /// FMIS Requisition Line Number
        /// </summary>
        public long FmisRequisitionLineNumber
        {
            get { return _fmisRequisitionLineNumber; }
        }

        /// <summary>
        /// FMIS Requisition Distribution Number
        /// </summary>
        public long FmisRequisitionDistributionNumber
        {
            get { return _fmisRequisitionDistributionNumber; }
        }

        /// <summary>
        /// Five-segment Distribution code populated in FMIS
        /// </summary>
        public string FmisDistribution
        {
            get { return _fmisDistribution; }
        }

        /// <summary>
        /// FMIS Vendor Id Number 
        /// </summary>
        public long FmisVendorCode
        {
            get { return _fmisVendorCode; }
        }

        /// <summary>
        /// FMIS Vendor Item Code
        /// </summary>
        public string FmisSupplierProductCode
        {
            get { return _fmisSupplierProductCode; }
        }

        /// <summary>
        /// FMIS Internal Item Number
        /// </summary>
        public string FmisHospitalProductCode
        {
            get { return _fmisHospitalProductCode; }
        }

        /// <summary>
        /// FMIS Price per unit of measure in AUD
        /// </summary>
        public double FmisPricePerUnit
        {
            get { return _fmisPricePerUnit; }
        }

        /// <summary>
        /// FMIS Unit of Measure
        /// </summary>
        public string FmisUnitOfMeasure
        {
            get { return _fmisUnitOfMeasure; }
        }

        /// <summary>
        /// FMIS ID to identify each Agency
        /// </summary>
        public string FmisOrgId
        {
            get { return _fmisOrgId; }
        }

        public override bool Equals(Object obj)
        {
            return obj is OraclePurchaseOrderReceipt &&
                   this == (OraclePurchaseOrderReceipt)obj;
        }

        public override int GetHashCode()
        {
            var hash = _orderId.GetHashCode() ^ 397;
            hash = hash ^ _orderLineId.GetHashCode();
            hash = hash ^ _fmisRequisitionNumber.GetHashCode();
            hash = hash ^ _fmisRequisitionLineNumber.GetHashCode();
            hash = hash ^ _fmisRequisitionDistributionNumber.GetHashCode();
            hash = hash ^ _fmisDistribution.GetHashCode();
            hash = hash ^ _fmisVendorCode.GetHashCode();
            hash = hash ^ _fmisSupplierProductCode.GetHashCode();
            hash = hash ^ _fmisHospitalProductCode.GetHashCode();
            hash = hash ^ _fmisPricePerUnit.GetHashCode();
            hash = hash ^ _fmisUnitOfMeasure.GetHashCode();
            hash = hash ^ _fmisOrgId.GetHashCode();

            return hash;
        }

        public static bool operator ==(OraclePurchaseOrderReceipt x, OraclePurchaseOrderReceipt y)
        {
            return x.OrderId == y.OrderId &&
                   x.OrderLineId == y.OrderLineId &&
                   x.FmisRequisitionNumber == y.FmisRequisitionNumber &&
                   x.FmisRequisitionLineNumber == y.FmisRequisitionLineNumber &&
                   x.FmisRequisitionDistributionNumber == y.FmisRequisitionDistributionNumber &&
                   x.FmisDistribution.Equals(y.FmisDistribution) &&
                   x.FmisVendorCode == y.FmisVendorCode &&
                   x.FmisSupplierProductCode.Equals(y.FmisSupplierProductCode) &&
                   x.FmisHospitalProductCode.Equals(y.FmisHospitalProductCode) &&
                   x.FmisPricePerUnit.Equals(y.FmisPricePerUnit) &&
                   x.FmisUnitOfMeasure.Equals(y.FmisUnitOfMeasure) &&
                   x.FmisOrgId == y.FmisOrgId;
        }

        public static bool operator !=(OraclePurchaseOrderReceipt x, OraclePurchaseOrderReceipt y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Convert string to T or throw <see cref="ArgumentException"/>
        /// </summary>
        private static T ConvertArgument<T>(IList<string> splitData, OraclePurchaseOrderField field)
        {
            return splitData[(int)field].ConvertArgument<T>(field.ToString());
        }
    }
}
