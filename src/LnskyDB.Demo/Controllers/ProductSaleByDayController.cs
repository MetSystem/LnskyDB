﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Tool;
using LnskyDB.Demo.Entity.Purify;
using LnskyDB.Demo.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LnskyDB.Demo.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProductSaleByDayController : ControllerBase
    {
        static int i = 0;
        private static IRepository<ProductSaleByDayEntity> GetRepository()
        {
            i++;
            if (i % 2 == 0)
            {
                //可以通过工厂创建
                return RepositoryFactory.Create<ProductSaleByDayEntity>();
            }
            else
            {
                //也可以继承实例化
                return new ProductSaleByDayRepository();
            }    
        }
        // GET http://localhost:53277/ProductSaleByDay/Get?sysNo=2d0c6662-9670-4c52-a4e1-02c37681e7e9

        [HttpGet]
        public ActionResult<ProductSaleByDayEntity> Get(Guid sysNo)
        {

            var repository = GetRepository();
            var entity = repository.Get(new ProductSaleByDayEntity
            {
                DBModel_ShuffledTempDate = new DateTime(2019, 01, 01),//这儿表示差19年1月的库和表
                SysNo = sysNo
            });
            if (entity == null)
            {
                return new ProductSaleByDayEntity();
            }
            return entity;
        }

        // GET http://localhost:53277/ProductSaleByDay/GetList?shopName=1&stTime=2019-01-01&endTime=2019-09-05
        [HttpGet]
        public ActionResult<List<ProductSaleByDayEntity>> GetList()
        {

            var stTime = new DateTime(2019, 1, 15);
            var endTime = new DateTime(2019, 2, 11);
            var repository = GetRepository();
            var query = QueryFactory.Create<ProductSaleByDayEntity>(m => m.ShopName.Contains("测试"));
            query.And(m => m.StatisticalDate >= stTime);
            query.And(m => m.StatisticalDate < endTime.Date.AddDays(1));
            query.DBModel.DBModel_ShuffledTempDate = new DateTime(2019, 01, 01);//这儿表示查19年1月的库和表
            query.OrderByDescing(m => m.StatisticalDate);
            query.StarSize = 20;
            query.Rows = 10;
            //分库的传入stTime,endTime会自动根据时间查询符合条件的库和表
            var lst = repository.GetList(query);

            return lst;
        }
        //GET http://localhost:53277/ProductSaleByDay/GetPaging?shopName=1&stTime=2019-01-01&endTime=2019-03-11&page=2&pageSize=3
        [HttpGet]
        public ActionResult<Paging<ProductSaleByDayEntity>> GetPaging()
        {

            var stTime = new DateTime(2019, 1, 15);
            var endTime = new DateTime(2019, 2, 11);
            var repository = GetRepository();
            var query = QueryFactory.Create<ProductSaleByDayEntity>(m => m.ShopName.Contains("测试"));
            query.And(m => m.StatisticalDate >= stTime);
            query.And(m => m.StatisticalDate < endTime.Date.AddDays(1));
            query.OrderByDescing(m => m.StatisticalDate);
            query.StarSize = 20;
            query.Rows = 10;
            //分库的传入stTime,endTime会自动根据时间查询符合条件的库和表
            var paging = repository.GetPaging(query, stTime, endTime);
            var count = paging.TotalCount;
            var lst = paging.ToList();//或者paging.Items
            return paging;
        }

        // Get http://localhost:53277/ProductSaleByDay/Add?shopName=增加2&StatisticalDate=2019-01-01&dataSource=测试用
        [HttpGet]
        public void Add()
        {
            var addEntity = new ProductSaleByDayEntity()
            {
                SysNo = Guid.NewGuid(),
                DataSource = "测试来源",
                ProductID = Guid.NewGuid(),
                ShopID = Guid.NewGuid(),
                ShopName = "测试店铺",
                ProductName = "测试商品",
                OutProductID = Guid.NewGuid().ToString(),
                ImportGroupId = Guid.NewGuid(),
                StatisticalDate = DateTime.Now//分库分表字段是必须的
            };
            var repository = GetRepository();
            //如果新增主键是自增列会自动赋值自增列值到主键
            repository.Add(addEntity);
        }
        //http://localhost:53277/ProductSaleByDay/Update?sysNo=650BC09C-2B9C-467B-A457-8B4853CC1F0F&shopName=修改&statisticalDate=2019-01-01&dataSource=修改测试用
        [HttpGet]
        public bool Update()
        {
            var updateEntity = new ProductSaleByDayEntity()
            {
                SysNo = Guid.Parse("650BC09C-2B9C-467B-A457-8B4853CC1F0F"),
                DataSource = "测试来源修改",
                ShopName = "店铺修改",
                StatisticalDate = new DateTime(2019, 01, 05),//如果StatisticalDate赋值了则根据StatisticalDate找库表,然后根据主键更新,StatisticalDate也会被更新成所赋的值
                //如果不想更新StatisticalDate可以用下面这句话
                // DBModel_ShuffledTempDate=new DateTime(2019,01,05),//如果不想更新StatisticalDate字段则用这句话来确定是那个库及表
            };
            var repository = GetRepository();
            //根据主键更新其他字段
            return repository.Update(updateEntity);
        }
        //http://localhost:53277/ProductSaleByDay/UpdateWhere?shopName=批量修改&shuffledTempDate=2019-02-02&dataSource=更新
        [HttpGet]
        public int UpdateWhere()
        {
            var updateEntity = new ProductSaleByDayEntity()
            {
                DataSource = "测试来源修改",
                ShopName = "店铺修改Where",
                DBModel_ShuffledTempDate = new DateTime(2019, 01, 05),//如果用这句话来确定是那个库及表
                // StatisticalDate = statisticalDate,//如果要更新StatisticalDate则可以用这句话替代上面那句话
            };
            var repository = GetRepository();
            var where = QueryFactory.Create<ProductSaleByDayEntity>(m => m.ShopName == "测试店铺1" && m.StatisticalDate > new DateTime(2019, 01, 03));//where是更新条件
            //注意如果是更新用的是实体类的DBModel_ShuffledTempDate Query中的无效
            return repository.Update(updateEntity, where);
        }
        // http://localhost:53277/ProductSaleByDay/Delete?sysNo=5119AC4F-FA2F-470F-8144-0069B45E15CF&statisticalDate=2019-01-01
        [HttpGet]
        public bool Delete()
        {

            var deleteEntity = new ProductSaleByDayEntity()
            {
                SysNo = Guid.Parse("650BC09C-2B9C-467B-A457-8B4853CC1F0F"),
                DBModel_ShuffledTempDate = new DateTime(2019, 01, 05),//对于分库分表来说DBModel_ShuffledTempDate是必须的用来确认是那个库表
            };
            var repository = GetRepository();
            return repository.Delete(deleteEntity);
        }

        //http://localhost:53277/ProductSaleByDay/DleteWhere?shopName=批量修改&shuffledTempDate=2019-02-02&dataSource=新+更
        [HttpGet]
        public int DleteWhere(string shopName, DateTime shuffledTempDate, string dataSource)
        {

            var repository = GetRepository();
            var where = QueryFactory.Create<ProductSaleByDayEntity>();
            where.DBModel.DBModel_ShuffledTempDate = new DateTime(2019, 01, 01);
            //QueryiSearch方法表示搜索里面空格表示或+表示且
            //如 导入+手工 自动+生成 表示字段必须同时拥有导入和手工或者自动和生成
            //生成sql是 and ((DataSource like '%导入%' and DataSource like '%手工%') or DataSource like '%自动%' and DataSource like '%生成%')            
            where.QueryiSearch(m => m.DataSource, "新+更");
            where.QueryiSearch(m => m.ShopName, "批量修改");
            //注意如果是更新用的是实体类的DBModel_ShuffledTempDate Query中的无效
            return repository.Delete(where);
        }


        [HttpGet]
        public void TestThread(string shopName, DateTime shuffledTempDate, string dataSource)
        {
            ThreadTool.QueueUserWorkItem(ThreadDo);
            ThreadTool.QueueUserWorkItem(ThreadDo);
            ThreadTool.QueueUserWorkItem(ThreadDo);
            ThreadTool.QueueUserWorkItem(ThreadDo);
            ThreadTool.QueueUserWorkItem(ThreadDo);
        }
        private static void ThreadDo()
        {
            var repository = GetRepository();
            for (int i = 0; i < 10; i++)
            {
                var stTime = DateTime.Now.Date.AddMonths(0 - i).AddDays(-3);

                var query = QueryFactory.Create<ProductSaleByDayEntity>();
                query.And(m => m.StatisticalDate >= stTime);
                query.And(m => m.StatisticalDate < stTime.AddDays(3));
                // query.DBModel.DBModel_ShuffledTempDate = new DateTime(2019, 01, 01);//这儿表示差19年1月的库和表
                //分页查询必须有排序字段
                query.OrderByDescing(m => m.StatisticalDate);
                //分库的传入stTime,endTime会自动根据时间查询符合条件的库和表
                var res = repository.GetList(query, stTime, stTime.AddDays(3));

            }
        }
    }
}
