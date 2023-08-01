
using System;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal struct OracleInboundFile
    {
        private readonly string _fileName;
        private readonly string _fileContents;

        public OracleInboundFile(string fileName, string fileContents)
        {
            _fileName = fileName;
            _fileContents = fileContents;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string FileContents
        {
            get { return _fileContents; }
        }

        public OracleInboundFileType InboundFileType 
        {
            get
            {
                return FileName.ToUpper().StartsWith("PO_") ? 
                    OracleInboundFileType.PurchaseOrder : OracleInboundFileType.ErrorReport;
            }
        }

        public override bool Equals(Object obj)
        {
            return obj is OracleInboundFile && 
                   this == (OracleInboundFile)obj;
        }

        public override int GetHashCode()
        {
            return _fileName.GetHashCode() ^ _fileContents.GetHashCode();
        }

        public static bool operator ==(OracleInboundFile x, OracleInboundFile y)
        {
            return x.FileName == y.FileName &&
                   x.FileContents == y.FileContents;
        }

        public static bool operator !=(OracleInboundFile x, OracleInboundFile y)
        {
            return !(x == y);
        }
    }
}
