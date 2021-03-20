using System;
using System.Collections.Generic;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.shepherdchurch.CubeDown
{
    [Serializable]
    public class ShoppingCartItem : Rock.Lava.ILiquidizable
    {
        #region Properties

        /// <summary>
        /// Gets the original content channel item.
        /// </summary>
        /// <value>
        /// The original content channel item.
        /// </value>
        [Newtonsoft.Json.JsonIgnore]
        public ContentChannelItem Item
        {
            get
            {
                if ( _item == null && ItemId.HasValue )
                {
                    _item = new ContentChannelItemService( new RockContext() ).Get( ItemId.Value );
                }

                return _item;
            }
        }
        [NonSerialized]
        private ContentChannelItem _item;

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [Newtonsoft.Json.JsonIgnore]
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the item identifier of the original content channel item.
        /// </summary>
        /// <value>
        /// The item identifier of the original content channel item.
        /// </value>
        public int? ItemId { get; set; }

        /// <summary>
        /// Gets or sets the name of this line item.
        /// </summary>
        /// <value>
        /// The name of this line item.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets the extended price, which is base price times quantity.
        /// </summary>
        /// <value>
        /// The extended price.
        /// </value>
        public decimal ExtendedPrice
        {
            get
            {
                return Price * Quantity;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is taxable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is taxable; otherwise, <c>false</c>.
        /// </value>
        public bool IsTaxable { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int? AccountId { get; set; }

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
                        "Item",
                        "ExtendedPrice",
                        "Guid",
                        "ItemId",
                        "Name",
                        "Price",
                        "Quantity",
                        "IsTaxable"
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
        [System.Runtime.CompilerServices.IndexerName( "IndexItem" )]
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
        /// Initializes a new instance of the <see cref="ShoppingCartItem"/> class.
        /// </summary>
        public ShoppingCartItem()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem"/> class.
        /// </summary>
        /// <param name="item">The content channel item to build from.</param>
        public ShoppingCartItem( ContentChannelItem item )
            : this()
        {
            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            ItemId = item.Id;
            Name = item.Title;
            Quantity = 1;
            Price = item.GetAttributeValue( "CubeDown.Price" ).AsDecimal();
            IsTaxable = item.GetAttributeValue( "CubeDown.Taxable" ).AsBoolean( false );

            //
            // If an Account Guid was provided, convert it to an Id number.
            //
            var accountGuid = item.GetAttributeValue( "CubeDown.Account" ).AsGuidOrNull();
            if ( accountGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var account = new FinancialAccountService( rockContext ).Get( accountGuid.Value );

                    AccountId = account?.Id;
                }
            }
        }

        #endregion
    }
}
