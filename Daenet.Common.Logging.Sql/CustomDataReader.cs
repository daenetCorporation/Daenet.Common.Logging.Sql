using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Daenet.Common.Logging.Sql
{
    class CustomDataReader : IDataReader
    {
        private IEnumerator<object[]> enumerator = null;

        private readonly List<object[]> data;

        public CustomDataReader(List<object[]> data)
        {
            this.enumerator = data.GetEnumerator();
            this.data = data;
        }

        #region Not needed Properties

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        #endregion

        public int FieldCount
        {
            get
            {
                if (enumerator.Current != null)
                {
                    return enumerator.Current.Length;
                }
                else
                    return data.First().Length;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool Read()
        {
            return enumerator.MoveNext();
        }

        public object GetValue(int i)
        {
            return enumerator.Current[i];
        }

        public int GetOrdinal(string name)
        {
            return Int32.Parse(name);
        }

        #region Not needed Methods

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }


        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
