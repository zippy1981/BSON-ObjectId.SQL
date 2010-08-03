// Taken from Sam Coder's MongoDB-csharp driver.
//
// Copyright 2009-2010 Sam Corder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
//		http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

/// <summary>
///   Oid is a struct object that represents a Mongo ObjectId. This is a SQL CLR type.
/// </summary>
/// <remarks>
/// Format.Native will not work here since the struct contains an array.
/// </remarks>
/// <see cref="http://keithelder.net/blog/archive/2007/10/29/Creating-Custom-SQL-CLR-UserDefined-Types.aspx"/>
[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined,IsByteOrdered=true,MaxByteSize=8000)]
public struct Oid: INullable, IBinarySerialize
{
    private static class BsonInfo
    {
        /// <summary>
        /// Initializes the <see cref="BsonInfo"/> class.
        /// </summary>
        static BsonInfo(){
            Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            MaxDocumentSize = 1024 * 1024 * 4; //4MB.
        }

        /// <summary>
        /// Gets or sets the epoch.
        /// </summary>
        /// <value>The epoch.</value>
        public static DateTime Epoch { get; private set; }
        
        /// <summary>
        /// The maximum size a document can be.
        /// </summary>
        public static int MaxDocumentSize { get; private set; }
    }
	
	private byte[] _bytes;
	
	/// <summary>
    ///   Initializes a new instance of the <see cref = "Oid" /> class.
    /// </summary>
    /// <param name = "value">The value.</param>
    public Oid(string value)
    {
        if(value == null)
            throw new ArgumentNullException("value");

        _bytes = ParseBytes(value);
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref = "Oid" /> class.
    /// </summary>
    /// <param name = "value">The value.</param>
    public Oid(byte[] value)
    {
        if(value == null)
            throw new ArgumentNullException("value");

        _bytes = new byte[12];
        Array.Copy(value, _bytes, 12);
    }

    /// <summary>
    ///   Gets the created.
    /// </summary>
    /// <value>The created.</value>
    public DateTime Created
    {
        get
        {
            var time = new byte[4];
            Array.Copy(_bytes, time, 4);
            Array.Reverse(time);
            var seconds = BitConverter.ToInt32(time, 0);
            return BsonInfo.Epoch.AddSeconds(seconds);
        }
    }
	
    /// <summary>Null check for SQL Server</summary>
	public bool IsNull {
		get {
			return _bytes == null;
		}
	}
    
    public static Oid Null
    {
        get
        {
        	return new Oid();
        }
    }
    
    public void Write(BinaryWriter w)
    {
        w.Write(_bytes);
    }

    public void Read(BinaryReader r)
    {
    	_bytes = r.ReadBytes(4);
    }

    /// <summary>
    ///   Compares the current object with another object of the same type.
    /// </summary>
    /// <param name = "other">An object to compare with this object.</param>
    /// <returns>
    ///   A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
    ///   Value
    ///   Meaning
    ///   Less than zero
    ///   This object is less than the <paramref name = "other" /> parameter.
    ///   Zero
    ///   This object is equal to <paramref name = "other" />.
    ///   Greater than zero
    ///   This object is greater than <paramref name = "other" />.
    /// </returns>
    public int CompareTo(Oid other)
    {
        if(ReferenceEquals(other, null))
            return 1;
        var otherBytes = other.ToByteArray();
        for(var x = 0; x < _bytes.Length; x++)
            if(_bytes[x] < otherBytes[x])
                return -1;
            else if(_bytes[x] > otherBytes[x])
                return 1;
        return 0;
    }

    /// <summary>
    ///   Decodes the hex.
    /// </summary>
    /// <param name = "value">The value.</param>
    /// <returns></returns>
    private static byte[] DecodeHex(string value)
    {
        var numberChars = value.Length;

        var bytes = new byte[numberChars/2];
        for(var i = 0; i < numberChars; i += 2)
            try
            {
                bytes[i/2] = Convert.ToByte(value.Substring(i, 2), 16);
            }
            catch
            {
                //failed to convert these 2 chars, they may contain illegal charracters
                bytes[i/2] = 0;
            }
        return bytes;
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name = "other">An object to compare with this object.</param>
    /// <returns>
    ///   true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(Oid other)
    {
        return CompareTo(other) == 0;
    }

    /// <summary>
    ///   Determines whether the specified <see cref = "System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name = "obj">The <see cref = "System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref = "System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if(obj is Oid)
            return CompareTo((Oid)obj) == 0;
        return false;
    }

    /// <summary>
    ///   Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///   A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
    
    public static Oid Parse(SqlString value)
    {
    	return new Oid(value.ToString());
    }

    /// <summary>
    /// Parses the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    private static byte[] ParseBytes(string value)
    {
        value = value.Replace("\"", "");
        ValidateHex(value);
        return DecodeHex(value);
    }

    /// <summary>
    ///   Returns a <see cref = "System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///   A <see cref = "System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
    	if (this.IsNull) { return "NULL"; }
    	else { return BitConverter.ToString(_bytes).Replace("-", "").ToLower(); }
    }

    /// <summary>
    ///   Converts the Oid to a byte array.
    /// </summary>
    public byte[] ToByteArray()
    {
        var ret = new byte[12];
        Array.Copy(_bytes, ret, 12);
        return ret;
    }

    /// <summary>
    ///   Validates the hex.
    /// </summary>
    /// <param name = "value">The value.</param>
    private static void ValidateHex(string value)
    {
        if(value == null || value.Length != 24)
            throw new ArgumentException("Oid strings should be 24 characters");

        var notHexChars = new Regex(@"[^A-Fa-f0-9]", RegexOptions.None);
        if(notHexChars.IsMatch(value))
            throw new ArgumentOutOfRangeException("value", "Value contains invalid characters");
    }
}