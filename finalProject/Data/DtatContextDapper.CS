using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace FINALPROJECT.Data
    {
    public class DataCountextDapper
    {
        private readonly IConfiguration _config;
        public DataCountextDapper(IConfiguration confg)
        {
            _config = confg;
        }
        public T ExecuteScalar<T>(string sql, object? parameters = null)
{
    using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
    {
#pragma warning disable CS8603 // Possible null reference return.
                return dbConnection.ExecuteScalar<T>(sql, parameters);
#pragma warning restore CS8603 // Possible null reference return.
            }
}

        public IEnumerable<T> loadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }
        public T loadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
            }
            public T loadDataSingle<T>(string sql, object? parameters = null)
{
    using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
    {
        return dbConnection.QuerySingle<T>(sql, parameters);
    }
}
        public bool executSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return (dbConnection.Execute(sql) > 0);
        }
        public int executSqlRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }
        public bool executSqlParameters(string sql, List<SqlParameter> Parameters)
        {
            SqlCommand commandWhithParams = new SqlCommand(sql);
            foreach (SqlParameter Parameter in Parameters)
            {
                commandWhithParams.Parameters.Add(Parameter);
            }
            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            dbConnection.Open();
            commandWhithParams.Connection = dbConnection;
            int RowsEffected = commandWhithParams.ExecuteNonQuery();
            dbConnection.Close();
            return RowsEffected > 0;
        }

    }
}