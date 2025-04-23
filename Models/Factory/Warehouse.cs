using Diamond.Models.Materials;
using Diamond.Models.Orders;
using Diamond.Models.Products;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diamond.Models.Factory
{
    public class Warehouse
    {
        #region Поля
        public int Id { get; set; }
        public int FactoryId { get; set; }

        [ForeignKey(nameof(FactoryId))]
        public Factory Factory { get; set; } = null!;
        public List<ProductSpecificWarehouse> Products { get; set; } = [];
        public List<MaterialWarehouse> Materials { get; set; } = [];
        #endregion

        #region Продукция
        public void AddProduct(int productId, int count, bool AddToRequest = false)
        {
            bool find = false;
            foreach (var product in Products)
                if (product.ProductId == productId)
                {
                    product.Count += count;
                    find = true;
                    break;
                }
            if (!find)
            {
                Products.Add(new()
                {
                    WarehouseId = Id,
                    ProductId = productId,
                    Count = count
                });
            }
            if (AddToRequest)
                AddToRequests();
        }
        public void AddProduct(Plan plan, bool AddToRequest = false)
        {
            bool find = false;
            foreach (var product in Products)
                if (product.ProductId == plan.ProductId)
                {
                    product.Count += plan.GetCountProduct;
                    find = true;
                    break;
                }
            if (!find)
            {
                Products.Add(new()
                {
                    WarehouseId = Id,
                    ProductId = plan.ProductId,
                    Count = plan.GetCountProduct
                });
            }
            if (AddToRequest)
                AddToRequests();
        }

        public void SubProduct(int productId, int count)
        {
            foreach (var product in Products)
                if (product.ProductId == productId)
                {
                    product.Count -= count;
                    if (product.Count <= 0)
                        Products.Remove(product);
                    return;
                }
        }
        #endregion

        #region Сырьё
        public void AddMaterial(int materialId, int count)
        {
            bool find = false;
            foreach (var material in Materials)
                if (material.MaterialId == materialId)
                {
                    material.Count += count;
                    find = true;
                    return;
                }
            if (!find)
            {
                Materials.Add(new()
                {
                    WarehouseId = Id,
                    MaterialId = materialId,
                    Count = count
                });
            }
        }

        public void SubMaterial(int materialId, int count)
        {
            foreach (var material in Materials)
                if (material.MaterialId == materialId)
                {
                    material.Count -= count;
                    if (material.Count <= 0)
                        Materials.Remove(material);
                    return;
                }
        }
        #endregion

        #region Работа с заявками
        public void AddToRequests()
        {
            List<Order> orders = [.. Factory.Orders
                .Where(r => r.Status == OrderStatus.FABRICATING)
                .OrderBy(r => r.DateOfDesiredComplete)];
            foreach (var order in orders)
            {
                foreach (var orderPart in order.OrderParts)
                    foreach (var product in Products)
                        if (orderPart.ProductId == product.ProductId)
                        {
                            int requestCountNeed = orderPart.Count - orderPart.CountComplete;
                            if (requestCountNeed >= product.Count) // Если продукции на складе меньше (или столько же), чем нужно
                            {
                                orderPart.CountComplete += product.Count;
                                Products.Remove(product);
                            }
                            else // Если продукции на склае больше, чем нужно
                            {
                                orderPart.CountComplete = orderPart.Count;
                                product.Count -= requestCountNeed;
                            }
                            break;
                        }
                order.UpdateStatus();
            }
        }
        #endregion
    }
}