﻿using Electronic.Api.DTO;
using Electronic.Api.Model;
using Electronic.Api.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Electronic.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBaseRepository<ApplicationUser> applicationUserRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IBaseRepository<SatusHistory> satusHistoryRepository;
        private readonly IBaseRepository<Cart> cartRepository;
        private readonly IBaseRepository<Product> productRepository;
        private readonly IBaseRepository<OrderProducts> orderproductsRepository;
        private readonly IBaseRepository<Order> order;
        private readonly ContextDB context;
        public OrderService(IBaseRepository<ApplicationUser> ApplicationUserRepository, UserManager<ApplicationUser> userManager, IBaseRepository<SatusHistory> SatusHistoryRepository, IBaseRepository<Cart> cartRepository, IBaseRepository<Product> productRepository, IBaseRepository<OrderProducts> orderproductsRepository, IBaseRepository<Order> _order, ContextDB _contextDB)
        {
            applicationUserRepository = ApplicationUserRepository;
            this.userManager = userManager;
            satusHistoryRepository = SatusHistoryRepository;
            this.cartRepository = cartRepository;
            this.productRepository = productRepository;
            this.orderproductsRepository = orderproductsRepository;
            order = _order;
            context = _contextDB;
        }

        public void AddNewOrder(string iduser, string payment, string Address, string State)
        {
            var Ord = new Order();


            Ord.CreateDate = DateTime.Now;
            Ord.UserID = iduser;
            Ord.TotalPrice = 0f;
            Ord.PaymentType = payment;
            Ord.SatusId = 1;
            Ord.Address = Address;
            Ord.State = State;
            Ord.DeliveryId = applicationUserRepository.GetByFirst(e => e.Address == "Delevary").Id;
            var result = order.Insert(Ord);



            var resallcart = cartRepository.GetAllwhere(e => e.UserID == Ord.UserID);



            foreach (var cart1 in resallcart)
            {
                OrderProducts orderproduct1 = new OrderProducts
                {
                    OrderID = result.Id,
                    OrderApprove = "Pending",
                    ProductID = cart1.ProductID,
                    Quantity = cart1.Quantity
                };

                orderproductsRepository.Insert(orderproduct1);

            }


            //var Ord2 = new Order();

            var orderproduct = orderproductsRepository.GetAllwhere(e => e.OrderID == result.Id);
            foreach (var item in orderproduct)
            {
                var res = productRepository.GetById(item.ProductID);
                Ord.TotalPrice += res.price * item.Quantity;
                res.CountProduct -= item.Quantity;
                productRepository.Update(res);

            }

            //Ord2.Id = result.Id;
            //Ord2.CreateDate = result.CreateDate;
            //Ord2.UserID = result.UserID;
            order.Update(Ord);

            var resdelet = cartRepository.GetAllwhere(e => e.UserID == iduser);
            cartRepository.DeleteRange(resdelet);

        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrders()
        {
            //return (IEnumerable<OrderDTO>)context.Orders.Include(a => a.ApplicationUser).ToList();
            var ords = order.GetAll();
            List<OrderDTO> OrdDTOs = new List<OrderDTO>();
            if (ords != null && ords.Count() > 0)
            {
                foreach (var ord in ords)
                {
                    var user = await userManager.FindByIdAsync(ord.UserID);
                    var dev = await userManager.FindByIdAsync(ord.DeliveryId);
                    OrderDTO order = new OrderDTO();

                    order.Id = ord.Id;
                    order.Total_Price = ord.TotalPrice;
                    order.Create_Date = ord.CreateDate;
                    order.User_Id = ord.UserID;
                    order.Count_Product = orderproductsRepository.GetAllwhere(e => e.OrderID == ord.Id).Sum(e => e.Quantity);
                    order.Satus = satusHistoryRepository.GetById(ord.SatusId).name;
                    order.OrderPlace_Date = ord.CreateDate.AddDays(4);
                    order.Name = user.UserName;
                    order.img = "https://localhost:7096/img/Users/" + user.Img;
                    order.Address = ord.Address;
                    order.State = ord.State;
                    order.DeliveryName = dev.UserName;
                    OrdDTOs.Add(order);
                }
                return OrdDTOs.OrderByDescending(e => e.Create_Date);
            }
            return null;
        }



        public async Task<IEnumerable<OrderDTO>> GetAllOrdersByUserId(string UserId)
        {
            //return (IEnumerable<OrderDTO>)context.Orders.Include(a => a.ApplicationUser).ToList();
            var user = await userManager.FindByIdAsync(UserId);
            var ords = order.GetAllwhere(e => e.UserID == UserId);
            List<OrderDTO> OrdDTOs = new List<OrderDTO>();
            if (ords != null && ords.Count() > 0)
            {
                foreach (var ord in ords)
                {

                    OrderDTO order = new OrderDTO();

                    order.Id = ord.Id;
                    order.Total_Price = ord.TotalPrice;
                    order.Create_Date = ord.CreateDate;
                    order.User_Id = ord.UserID;
                    order.Count_Product = orderproductsRepository.GetAllwhere(e => e.OrderID == ord.Id).Sum(e => e.Quantity);
                    order.Satus = satusHistoryRepository.GetById(ord.SatusId).name;
                    order.OrderPlace_Date = ord.CreateDate.AddDays(4);
                    order.Name = user.UserName;
                    order.img = "https://localhost:7096/img/Users/" + user.Img;
                    OrdDTOs.Add(order);
                }
                return OrdDTOs.OrderByDescending(e => e.Create_Date);
            }
            return null;
        }


        public async Task<OrderDTO> GetOrderById(int Id)
        {
            var ord = order.GetById(Id);
            var user = await userManager.FindByIdAsync(ord.UserID);
            if (ord != null)
            {
                OrderDTO order = new OrderDTO();


                order.Id = ord.Id;
                order.Total_Price = ord.TotalPrice;
                order.Create_Date = ord.CreateDate;
                order.User_Id = ord.UserID;
                order.Count_Product = orderproductsRepository.GetAllwhere(e => e.OrderID == ord.Id).Sum(e => e.Quantity);
                order.Satus = satusHistoryRepository.GetById(ord.SatusId).name;
                order.OrderPlace_Date = ord.CreateDate.AddDays(4);
                order.Name = user.UserName;
                order.img = "https://localhost:7096/img/Users/" + user.Img;
                order.Address = ord.Address;
                order.State = ord.State;
                order.DeliveryName = applicationUserRepository.GetByFirst(e => e.Id == ord.DeliveryId).UserName;
                return order;
            }
            return null;
        }

        public void RemoveOrder(int Id)
        {
            var Ord = order.GetById(Id);
            if (Ord != null)
            {
                order.Delete(Ord);
            }
        }

        public void UpdateOrder(OrderDTO OrdDTO)
        {
            var Ord = new Order();

            Ord.Id = OrdDTO.Id;
            Ord.TotalPrice = OrdDTO.Total_Price;
            Ord.CreateDate = OrdDTO.Create_Date;
            Ord.UserID = OrdDTO.User_Id;
            order.Update(Ord);
        }
        public IEnumerable<Order> GetOrdersByUserId(string UserId)
        {
            return context.Orders.Where(o => o.UserID == UserId);
        }

        public bool UpdateStatusOrder(int IdOrder, string Status)
        {
            var satusH = satusHistoryRepository.GetByFirst(e => e.name.ToLower() == Status.ToLower());
            var ord = order.GetById(IdOrder);

            ord.SatusId = satusH.Id;
            order.Update(ord);
            return true;

        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersByDelevary(string UserId)
        {
            //return (IEnumerable<OrderDTO>)context.Orders.Include(a => a.ApplicationUser).ToList();
            var delevary = await userManager.FindByIdAsync(UserId);
            var ords = order.GetAllwhere(e => e.DeliveryId == UserId);
            List<OrderDTO> OrdDTOs = new List<OrderDTO>();
            if (ords != null && ords.Count() > 0)
            {
                foreach (var ord in ords)
                {
                    var user = await userManager.FindByIdAsync(ord.UserID);


                    OrderDTO order = new OrderDTO();

                    order.Id = ord.Id;
                    order.Total_Price = ord.TotalPrice;
                    order.Create_Date = ord.CreateDate;
                    order.User_Id = ord.UserID;
                    order.Count_Product = orderproductsRepository.GetAllwhere(e => e.OrderID == ord.Id).Sum(e => e.Quantity);
                    order.Satus = satusHistoryRepository.GetById(ord.SatusId).name;
                    order.OrderPlace_Date = ord.CreateDate.AddDays(4);
                    order.Name = user.UserName;
                    order.img = "https://localhost:7096/img/Users/" + user.Img;
                    OrdDTOs.Add(order);
                }
                return OrdDTOs.OrderByDescending(e => e.Create_Date);
            }
            return null;
        }

        /*public int Count()
        {
            return (int)context.Orders.Sum(e => e.TotalPrice);
        }*/
    }
}
