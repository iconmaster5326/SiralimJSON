using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    public interface IShopItem
    {
        public QuickType.ShopItemType ShopItemType { get; }
    }
    public class ShopItemSpecial : IShopItem, IEquatable<ShopItemSpecial?>
    {
        public virtual QuickType.ShopItemType ShopItemType => throw new NotImplementedException();

        public override bool Equals(object? obj)
        {
            return Equals(obj as ShopItemSpecial);
        }

        public bool Equals(ShopItemSpecial? other)
        {
            return other is not null;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(ShopItemSpecial? left, ShopItemSpecial? right)
        {
            return EqualityComparer<ShopItemSpecial>.Default.Equals(left, right);
        }

        public static bool operator !=(ShopItemSpecial? left, ShopItemSpecial? right)
        {
            return !(left == right);
        }
    }
    public class ShopItemGOTGKey : ShopItemSpecial {
        public override QuickType.ShopItemType ShopItemType => QuickType.ShopItemType.GotgKey;
    }
    public class ShopItemSmallChest : ShopItemSpecial {
        public override QuickType.ShopItemType ShopItemType => QuickType.ShopItemType.SmallChest;
    }
    public class ShopItemMediumChest : ShopItemSpecial {
        public override QuickType.ShopItemType ShopItemType => QuickType.ShopItemType.MediumChest;
    }
    public class ShopItemLargeChest : ShopItemSpecial {
        public override QuickType.ShopItemType ShopItemType => QuickType.ShopItemType.LargeChest;
    }
    public class ShopEntry : IEquatable<ShopEntry?>
    {
        public required int Cost;
        public required IShopItem Item;

        public override bool Equals(object? obj)
        {
            return Equals(obj as ShopEntry);
        }

        public bool Equals(ShopEntry? other)
        {
            return other is not null &&
                   Cost == other.Cost &&
                   EqualityComparer<IShopItem>.Default.Equals(Item, other.Item);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Cost, Item);
        }

        public static bool operator ==(ShopEntry? left, ShopEntry? right)
        {
            return EqualityComparer<ShopEntry>.Default.Equals(left, right);
        }

        public static bool operator !=(ShopEntry? left, ShopEntry? right)
        {
            return !(left == right);
        }

        public QuickType.ShopItem AsJSON
        {
            get
            {
                if (Item is ISiralimEntity)
                {
                    return new()
                    {
                        Type = Item.ShopItemType,
                        Cost = Cost,
                        Id = (int) ((ISiralimEntity)Item).Key,
                    };
                } else
                {
                    return new()
                    {
                        Type = Item.ShopItemType,
                        Cost = Cost,
                    };
                }
            }
        }
    }
    public class Shop
    {
        public List<ShopEntry> Items = new();

        public Shop(GameVariable var)
        {
            var items = var.InstanceVar("items").GetArray().ToList();
            var costs = var.InstanceVar("cost").GetArray().Select(x => x.GetInt32()).ToList();
            int nItems = Math.Min(items.Count, costs.Count);

            for (int i = 1; i < nItems; i++)
            {
                var item = items[i];
                IShopItem parsedItem;

                switch (item.InstanceObjectName())
                {
                    case "obj_consumable":
                        int param = item.InstanceVar("param");
                        switch (item.InstanceVar("itemid").GetInt32())
                        {
                            case 117:
                                parsedItem = Spell.Database[param];
                                break;
                            case 140:
                                parsedItem = Decoration.Database[param];
                                break;
                            case 141:
                                parsedItem = DecorationMusic.Database[param];
                                break;
                            case 142: // TODO: is this correct?
                                parsedItem = DecorationWalls.Database[param];
                                break;
                            case 143: // TODO: is this correct?
                                parsedItem = DecorationFloors.Database[param];
                                break;
                            case 144:
                                parsedItem = DecorationBackground.Database[param];
                                break;
                            case 146:
                                parsedItem = ProjectItem.Database[param];
                                break;
                            case 153:
                                parsedItem = Creature.Database[param];
                                break;
                            case 157:
                                parsedItem = new ShopItemSmallChest();
                                break;
                            case 159:
                                parsedItem = new ShopItemMediumChest();
                                break;
                            case 160:
                                parsedItem = new ShopItemLargeChest();
                                break;
                            default:
                                throw new Exception($"Unknown obj_consumable ID {item.InstanceVar("itemid").GetInt32()} with param {param} in shop!");
                        }
                        break;
                    case "obj_material":
                        parsedItem = ItemMaterial.Database[item.InstanceVar("matid")];
                        break;
                    case "obj_dust":
                        parsedItem = ItemSpellProperty.Database[item.InstanceVar("itemid")];
                        break;
                    case "obj_misc":
                        switch (item.InstanceVar("itemid").GetInt32())
                        {
                            case 131:
                                parsedItem = new ShopItemGOTGKey();
                                break;
                            default:
                                throw new Exception($"Unknown obj_misc ID {item.InstanceVar("itemid").GetInt32()} in shop!");
                        }
                        break;
                    default:
                        throw new Exception($"Unknown item in shop of type '{item.InstanceObjectName()}'!");
                }

                Items.Add(new()
                {
                    Cost = costs[i],
                    Item = parsedItem,
                });
            }
        }
    }
}
