using System;
using System.Collections.Generic;
using System.Linq;

namespace com.shepherdchurch.CubeDown
{
    [Serializable]
    public class ShoppingCart : Rock.Lava.ILiquidizable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the receipt code.
        /// </summary>
        /// <value>
        /// The receipt code.
        /// </value>
        public string ReceiptCode { get; set; }

        /// <summary>
        /// Gets or sets the subtotal.
        /// </summary>
        /// <value>
        /// The subtotal.
        /// </value>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Gets or sets the tax rate.
        /// </summary>
        /// <value>
        /// The tax rate.
        /// </value>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Gets or sets the tax.
        /// </summary>
        /// <value>
        /// The tax.
        /// </value>
        public decimal Tax { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public decimal Total { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public List<ShoppingCartItem> Items { get; set; }

        #endregion

        #region ILiquidizable Interface

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [Newtonsoft.Json.JsonIgnore]
        public List<string> AvailableKeys
        {
            get
            {
                return new List<string>
                    {
                        "ReceiptCode",
                        "Subtotal",
                        "Total",
                        "AmountDue",
                        "Items",
                        "TaxRate",
                        "Tax"
                    };
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                var pi = GetType().GetProperty( key.ToString() );

                return pi?.GetValue( this );
            }
        }

        /// <summary>
        /// Retrieves an object that can be used to represent this object in Lava.
        /// </summary>
        /// <returns>An object that can be used to represent this object in Lava</returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Determines whether the lava object contains the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the lava object contains the key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey( object key )
        {
            return AvailableKeys.Contains( key.ToString() );
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
        /// </summary>
        public ShoppingCart()
        {
            Items = new List<ShoppingCartItem>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates all the subtotal and total values based on the item list.
        /// </summary>
        public void Calculate()
        {
            Subtotal = Items.Sum( i => i.Price * i.Quantity );
            Tax = Math.Round( Items.Where( i => i.IsTaxable ).Sum( i => i.Price * i.Quantity ) * TaxRate / 100, 2 );
            Total = Subtotal + Tax;
        }

        #endregion
    }
}
