﻿using MongoDB.Driver;
using OfficeOpenXml;
using PIDI.App_Start;
using PIDI.Models;
using PIDI.Models.Commom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIDI.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    public class RelatorioController : Controller
    {

        public static RelatorioController Instance
        {
            get
            {
                return GetInstace();
            }
            set { }
        }
        private static RelatorioController _instance;

        private MongoDBContext dBContext;
        public static List<PedidosModel> lastOrderRequest = new List<PedidosModel>();
        public static List<UserModel> lastUserRequest = new List<UserModel>();

        private static RelatorioController GetInstace()
        {
            if (_instance == null)
            {
                _instance = new RelatorioController();
                return _instance;
            }
            else
                return _instance;
        }

        #region Collections Reference

        private IMongoCollection<ProductModel> productCollection;
        private IMongoCollection<PedidosModel> orderCollection;
        private IMongoCollection<UserModel> userCollection;

        #endregion

        public RelatorioController()
        {
            dBContext = new MongoDBContext();

            productCollection = dBContext.database.GetCollection<ProductModel>("product");
            orderCollection = dBContext.database.GetCollection<PedidosModel>("orders");
            userCollection = dBContext.database.GetCollection<UserModel>("users");

        }

        #region Relatorio Pedido
        /// <summary>
        /// Gerar relatório de pedidos
        /// </summary>
        /// <returns></returns>
        public ActionResult GerarRelatorioPedido(DateTime dtInicio  , DateTime dtFinal)
        {
            var pedidos = orderCollection.AsQueryable().ToList();
            var filtered = pedidos.FindAll(x => x.OrderDate.Date <= dtFinal && x.OrderDate.Date >= dtInicio);
            lastOrderRequest = filtered;
            return View(filtered);
        }

        [HttpPost]
        public ActionResult GerarRelatorioPedido(DateTime dtInicio, DateTime dtFinal, string orderState)
        {
            var pedidos = orderCollection.AsQueryable().ToList();
            var filtered = pedidos.FindAll(x => x.OrderDate.Date <= dtFinal && x.OrderDate.Date >= dtInicio && x.orderState == orderState);
            lastOrderRequest = filtered;
            return View(filtered);
        }

        public void DownloadPedidosExcel()
        {
            var collection = lastOrderRequest;//db.GetCollection<EmployeeDetails>("EmployeeDetails").Find(new BsonDocument()).ToList();

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            Sheet.Cells["A1"].Value = "Order ID";
            Sheet.Cells["B1"].Value = "User ID";
            Sheet.Cells["C1"].Value = "State";
            Sheet.Cells["D1"].Value = "Requested Products";
            Sheet.Cells["E1"].Value = "Order State";
            Sheet.Cells["F1"].Value = "Total";

            int row = 2;
            foreach (var item in collection)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.OrderId;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.userId;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.State;

                string produtos = "";
                for (int i = 0; i < item.produtosRequisitados.Count; i++)
                {
                    produtos += item.produtosRequisitados[i].produtoRequisitado.ProductName;
                    produtos += " x " + item.produtosRequisitados[i].Quantity + " \n ";
                }

                Sheet.Cells[string.Format("D{0}", row)].Style.WrapText = true;
                Sheet.Cells[string.Format("D{0}", row)].Value = produtos;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.orderState;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Total;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "RelatorioPedido.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        #endregion

        #region Relatório Clientes
        /// <summary>
        /// Gerar relatório de pedidos
        /// </summary>
        /// <returns></returns>
        public ActionResult GerarRelatorioCliente(DateTime dtInicio, DateTime dtFinal)
        {
            var pedidos = userCollection.AsQueryable().ToList();
            var filtered = pedidos.FindAll(x => x.dtCriacao.Date <= dtFinal && x.dtCriacao.Date >= dtInicio);
            lastUserRequest = filtered;
            return View(filtered);
        }

        [HttpPost]
        public ActionResult GerarRelatorioCliente(DateTime dtInicio, DateTime dtFinal, string clientState)
        {
            var clientes = userCollection.AsQueryable().ToList();
            var filtered = clientes.FindAll(x => x.dtCriacao.Date <= dtFinal.Date && x.dtCriacao.Date >= dtInicio.Date );
            lastUserRequest = filtered;
            return View(filtered);
        }

        public void DownloadClienteExcel()
        {
            var collection = lastUserRequest;//db.GetCollection<EmployeeDetails>("EmployeeDetails").Find(new BsonDocument()).ToList();

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            Sheet.Cells["A1"].Value = "User ID";
            Sheet.Cells["B1"].Value = "Nome";
            Sheet.Cells["C1"].Value = "Sexo";
            Sheet.Cells["D1"].Value = "Email";
            Sheet.Cells["E1"].Value = "Data Criação";
            Sheet.Cells["F1"].Value = "Data Nascimento";

            int row = 2;
            foreach (var item in collection)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.Id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.nome;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.sexo;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.email;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.dtCriacao;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.dtNascimento;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "RelatorioPedido.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        #endregion
    }
}