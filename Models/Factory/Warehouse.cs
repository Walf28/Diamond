using Diamond.Models.Materials;
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
                    product.Count += plan.Size;
                    find = true;
                    break;
                }
            if (!find)
            {
                Products.Add(new()
                {
                    WarehouseId = Id,
                    ProductId = plan.ProductId,
                    Count = plan.Size
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
            List<Request> requests = [.. Factory.Requests.OrderBy(r => r.DateOfDesiredComplete)];
            foreach (var request in requests)
            {
                foreach (var product in  Products)
                    if (request.ProductId == product.ProductId)
                    {
                        if (request.Count >= product.Count)
                        {
                            request.CountComplete += product.Count;
                            Products.Remove(product);
                        }
                        else
                        {
                            request.CountComplete += request.Count;
                            product.Count -= product.Count;
                        }
                        break;
                    }
                request.UpdateStatus();
            }
        }
        #endregion
    }
}