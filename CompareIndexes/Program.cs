using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CompareIndexes
{
    internal class Program
    {
        static DataTable sourceDt, destinationDt;
        static string sqlquery2 = "SELECT TableName = t.name,IndexName = ind.name,     ColumnName = col.name FROM sys.indexes ind INNER JOIN      sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id INNER JOIN      sys.tables t ON ind.object_id = t.object_id WHERE      ind.is_primary_key = 0 AND ind.is_unique = 0 AND ind.is_unique_constraint = 0 AND t.is_ms_shipped = 0 ORDER BY      t.name, ind.name, ind.index_id, ic.is_included_column, ic.key_ordinal;";

        static string sqlquery = "SELECT TableName = t.name,IndexName = ind.name,     ColumnName = col.name FROM sys.indexes ind INNER JOIN      sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id INNER JOIN      sys.tables t ON ind.object_id = t.object_id ORDER BY      t.name, ind.name, ind.index_id, ic.is_included_column, ic.key_ordinal;";

        static void Main(string[] args)
        {
            GetIndices("sourcedb");
            GetIndices("seconddb");

            CompareIndices();
        }

        private static void GetIndices(string connstring)
        {
            string sourceConnString = ConfigurationManager.AppSettings[connstring];

            using SqlConnection conn = new(sourceConnString);
            conn.Open();
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sqlquery;

            using SqlDataReader reader = cmd.ExecuteReader();
            if (connstring == "sourcedb")
            {
                sourceDt = new DataTable();
                sourceDt.Load(reader);
            }
            else if (connstring == "seconddb")
            {
                destinationDt = new DataTable();
                destinationDt.Load(reader);
            }
        }

        private static void CompareIndices()
        {
            Console.WriteLine("\nSource Db: " + ConfigurationManager.AppSettings["sourcedb"]);
            Console.WriteLine("Destination Db: " + ConfigurationManager.AppSettings["seconddb"]);

            Console.WriteLine("----------------------------------------");
            //contains in sourceDb but not in destination
            Console.WriteLine("contains in SourceDb but not in DestinationDb");
            foreach (DataRow sourceDr in sourceDt.Rows)
            {
                var dr = destinationDt.Select("TableName = '" + sourceDr["TableName"] + "' and ColumnName='" + sourceDr["ColumnName"] + "'");

                if (dr.Length == 0)
                {
                    Console.WriteLine(string.Format("|{0,32}|{1,32}|{2,32}|", sourceDr["TableName"] , sourceDr["IndexName"] , sourceDr["ColumnName"]));
                }
            }

            Console.WriteLine("----------------------------------------");

            //contains in destinationDb but not in sourcedb
            Console.WriteLine("contains in destinationDb but not in sourcedb");
            foreach (DataRow destinationDr in destinationDt.Rows)
            {
                var dr = sourceDt.Select("TableName = '" + destinationDr["TableName"] + "' and ColumnName='" + destinationDr["ColumnName"] + "'");

                if (dr.Length == 0)
                {
                    Console.WriteLine(string.Format("|{0,32}|{1,32}|{2,32}|", destinationDr["TableName"] ,destinationDr["IndexName"] , destinationDr["ColumnName"]));
                }
            }

            Console.ReadLine();
        }
    }
}