using System;
using System.Linq;

using Rock;
using Rock.Model;

namespace com.shepherdchurch.CubeDown
{
    public static class ReceiptHelper
    {
        #region Fields

        /// <summary>
        /// The base16 map used for custom hex encoding.
        /// </summary>
        static char[] Base16Map = new char[16]
        {
                'S', 'B', 'N', 'D',
                'R', 'F', 'G', 'C',
                'T', 'J', 'K', 'M',
                'W', 'P', 'Q', 'X'
        };

        #endregion

        /// <summary>
        /// Encodes the receipt code.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <returns>An encoded string that represents the receipt identifier.</returns>
        public static string EncodeReceiptCode( InteractionComponent receipt )
        {
            var receiptSecret = (ushort)string.Format( "{0:00}{0:000}", receipt.CreatedDateTime.Value.Second, receipt.CreatedDateTime.Value.Millisecond ).AsInteger();

            return EncodeReceiptCode( receipt.Id, receiptSecret );
        }

        /// <summary>
        /// Encodes the receipt code from the receipt identifier and secret value.
        /// </summary>
        /// <param name="receiptId">The receipt identifier.</param>
        /// <param name="secret">The secret value.</param>
        /// <returns>An encoded string that represents the receipt identifier.</returns>
        public static string EncodeReceiptCode( int receiptId, ushort secret )
        {
            //
            // Get the 2 byte values that represent the secret. If we are on a little
            // endian platform then swap to network byte order.
            //
            var bytes = BitConverter.GetBytes( secret );
            if ( BitConverter.IsLittleEndian )
            {
                bytes = bytes.Reverse().ToArray();
            }

            //
            // Walk each byte and convert to our custom hex string.
            //
            string secretStr = "";
            foreach ( var b in bytes )
            {
                secretStr += new string( new char[] { Base16Map[b & 0xf], Base16Map[b >> 4] } );
            }

            return secretStr + receiptId.ToString();
        }

        /// <summary>
        /// Decodes the receipt code to it's receipt identifier and secret value.
        /// </summary>
        /// <param name="receiptCode">The receipt code.</param>
        /// <param name="secret">The secret.</param>
        /// <returns>The receipt identifier.</returns>
        public static int? DecodeReceiptCode( string receiptCode, out ushort? secret )
        {
            //
            // Verify this is a valid code.
            //
            if ( receiptCode == null || receiptCode.Length < 5 )
            {
                secret = null;
                return null;
            }

            //
            // Get the hex-encoded secret segment.
            //
            var base16Values = receiptCode.Substring( 0, 4 )
                .ToUpper()
                .Select( c => Array.IndexOf( Base16Map, c ) )
                .ToArray();

            //
            // Convert the custom hex value back to raw byte values.
            //
            var bytes = new byte[2]
            {
                (byte)( base16Values[0] + ( base16Values[1] << 4 ) ),
                (byte)( base16Values[2] + ( base16Values[3] << 4 ) )
            };

            //
            // If we are on a little endian platform then convert back from
            // network byte order.
            //
            if ( BitConverter.IsLittleEndian )
            {
                bytes = bytes.Reverse().ToArray();
            }

            //
            // Convert the secret back to an integer value.
            //
            secret = BitConverter.ToUInt16( bytes, 0 );

            return receiptCode.Substring( 4 ).AsIntegerOrNull();
        }

        /// <summary>
        /// Validates the receipt code matches the database receipt.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <param name="receiptCode">The receipt code.</param>
        /// <returns><c>true</c> if the receipt code is valid.</returns>
        public static bool ValidateReceiptCode( InteractionComponent receipt, string receiptCode )
        {
            return EncodeReceiptCode( receipt ) == receiptCode.ToUpper();
        }
    }
}
