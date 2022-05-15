using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace Sql_Query
{
    static class Program
    {
        public enum DataBase { Users, Data };
        static void Main(string[] args)
        {
            List<Data> data = new List<Data>();
            List<User> users = new List<User>();
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("Please enter your query");
                Console.WriteLine("The format is: from <Source> where <Experession> select <Field>");
                string user_input = Console.ReadLine();
                //the user is input by the format from<> where<> select<>, I split all part to another string array
                string[] query = parser(user_input);
                DataBase chooseCase = query[1] == "Users" ? DataBase.Users : DataBase.Data;
                string[] filters = query[3].Split(" ");

                switch (chooseCase)
                {
                    case DataBase.Users:
                        if (filters.Length > 3)//there are complex condition - but not more than 3 conditions!
                        {
                            List<User> part1 = new List<User>();
                            List<User> part2 = new List<User>();
                            List<User> part3 = new List<User>();
                            bool block = false;
                            if (filters[4][0] == '(' && filters.Length > 7)
                            { //there is () in the query so the order of the operation is changed
                                filters[4] = filters[4].Split('(')[1];
                                filters[10] = filters[10].Split(')')[0];
                                part1 = getPartUserQuery(new string[] { filters[8], filters[9], filters[10] });
                                block = true;
                            }
                            else
                                part1 = getPartUserQuery(new string[] { filters[0], filters[1], filters[2] });
                            part2 = getPartUserQuery(new string[] { filters[4], filters[5], filters[6] });
                            //if there are () at the query take the operator in index 7, else take from index 3
                            users = userComplexCondition(part1, part2, filters[block ? 7 : 3]);
                            if (filters.Length > 7)//there are 3 conditions
                            {
                                if (block)
                                    //there are () at the end of query, so the order is changed, and the first filter is the last
                                    part3 = getPartUserQuery(new string[] { filters[0], filters[1], filters[2] });
                                else
                                    part3 = getPartUserQuery(new string[] { filters[8], filters[9], filters[10] });
                                //if there are () at the query take the operator in index 3, else take from index 7
                                users = userComplexCondition(users, part3, filters[block ? 3 : 7]);
                            }
                        }
                        else
                        {//there is only one condition, (filter, operator and word for seacrh)
                            users = getPartUserQuery(filters);
                        }
                        string[] print = query[5].Split(',');
                        foreach (var item in users)//print the results by the user query
                        {
                            if (print.Length == 3 || print[0] == "*" || print[0] == " *")
                                Console.WriteLine(item.ToString());
                            else
                            {
                                for (int i = 0; i < print.Length; i++)
                                {
                                    if (print[i] == "Email" || print[i] == " Email")
                                        Console.WriteLine($"Email: {item.Email}");
                                    if (print[i] == "FullName" || print[i] == " FullName")
                                        Console.WriteLine($"FullName: {item.FullName}");
                                    if (print[i] == "Age" || print[i] == " Age")
                                        Console.WriteLine($"Age: {item.Age}");
                                }
                            }
                        }
                        break;
                    case DataBase.Data:
                        if (filters.Length > 3)//there are complex condition - but not more than 3 conditions!
                        {
                            List<Data> part1 = new List<Data>();
                            List<Data> part2 = new List<Data>();
                            List<Data> part3 = new List<Data>();
                            bool block = false;
                            if (filters[4][0] == '(' && filters.Length > 7)
                            { //there is () in the query so the order of the operation is changed
                                filters[4] = filters[4].Split('(')[1];
                                filters[10] = filters[10].Split(')')[0];
                                part1 = getPartDataQuery(new string[] { filters[8], filters[9], filters[10] });
                                block = true;
                            }
                            else
                                part1 = getPartDataQuery(new string[] { filters[0], filters[1], filters[2] });
                            part2 = getPartDataQuery(new string[] { filters[4], filters[5], filters[6] });
                            data = dataComplexCondition(part1, part2, filters[block ? 7 : 3]);
                            if (filters.Length > 7)//there are 3 conditions
                            {
                                if (block)
                                    //there are () at the end of query, so the order is changed, and the first filter is the last
                                    part3 = getPartDataQuery(new string[] { filters[0], filters[1], filters[2] });
                                else
                                    part3 = getPartDataQuery(new string[] { filters[8], filters[9], filters[10] });

                                data = dataComplexCondition(data, part3, filters[block ? 3 : 7]);
                            }
                        }
                        else
                            data = getPartDataQuery(filters);


                        print = query[5].Split(',');
                        foreach (var item in data)//print the results by the user query
                        {
                            for (int i = 0; i < print.Length; i++)
                            {
                                if (print[i] == "Users" || print[i] == " Users")
                                    foreach (var user in item.users)
                                        Console.WriteLine(user.ToString());
                                if (print[i] == "Orders" || print[i] == " Orders")
                                    foreach (var order in item.orders)
                                    {
                                        Console.WriteLine(order.ToString());
                                    }
                                if(print[i] == "*" || print[i] == " *")
                                {
                                    foreach(var user in item.users)
                                        Console.WriteLine(user.ToString());
                                    foreach(var order in item.orders)
                                        Console.WriteLine(order.ToString());
                                }
                                    
                            }
                        }
                        break;
                    default:
                        flag = false;
                        break;
                }
                query = new string[] { };
            }
        }


        private static string[] parser(string user_input)
        {
            try
            {
                string[] parse = user_input.Split("where ");
                string from = "from ";
                string DB = parse[0].Split(" ")[1];
                string where = "where ";
                string condition = parse[1].Split(" select")[0];
                string select = "Select ";
                string filter = parse[1].Split(" select")[1];
                return new string[] { from, DB, where, condition, select, filter };
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR! wrong input");
                return new string[] { "from ", "", "where ", "", " select", "" };
            }
        }
        #region initialization
        private static List<Data> initilizeDataDB()
        {
            List<Data> dataList = new List<Data>();
            SqlConnection connection = new SqlConnection("Data Source=DESKTOP-6PQJSFF;Initial Catalog=test_db;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select * from Orders", connection);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            Data data = new Data();
            data.users = initilizeUserDB();
            data.orders = ConvertToOrdersList(dt);
            dataList.Add(data);
            return dataList;
        }
        private static List<User> initilizeUserDB()
        {
            SqlConnection connection = new SqlConnection("Data Source=DESKTOP-6PQJSFF;Initial Catalog=test_db;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select * from Users", connection);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return ConvertToUserList(dt);
        }
        private static List<Order> initilizeOrderDB()
        {
            SqlConnection connection = new SqlConnection("Data Source=DESKTOP-6PQJSFF;Initial Catalog=test_db;Integrated Security=True");
            SqlCommand cmd = new SqlCommand("Select * from Orders", connection);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return ConvertToOrderList(dt);
        }
        #endregion
        #region complex condition
        private static List<User> userComplexCondition(List<User> part1, List<User> part2, string v)
        {
            if (v == "or")
            {
                return orLogical(part1, part2);
            }
            if (v == "and")
            {
                return andLogical(part1, part2);
            }
            return null;
        }
        private static List<Data> dataComplexCondition(List<Data> part1, List<Data> part2, string v)
        {
            List<User> users = new List<User>();
            List<Order> orders = new List<Order>();
            if (v == "or")
            {
                orders = orLogicalOrder(part1[0].orders, part2[0].orders);
                users = orLogical(part1[0].users, part2[0].users);
            }
            if (v == "and")
            {
                orders = andLogicalOrder(part1[0].orders, part2[0].orders);
                users = andLogical(part1[0].users, part2[0].users);

            }
            List<Data> ans = new List<Data>();
            Data dt = new Data();
            dt.orders = orders;
            dt.users = users;
            ans.Add(dt);
            return ans;
        }
        #endregion
        #region operators
        public static Boolean Operator(this string logic, int x, int y)
        {
            switch (logic)
            {
                case ">": return x > y;
                case ">=": return x >= y;
                case "<": return x < y;
                case "<=": return x <= y;
                case "==": return x == y;
                default: throw new Exception("invalid logic");
            }
        }
        private static List<User> andLogical(List<User> part1, List<User> part2)
        {
            if (part1.Count == 0 || part2.Count == 0)//is there are one condition false the and logical is false
                return new List<User>();
            else
            {    //the two condition is true, return all this two results without duplicate
                List<User> ans = new List<User>();
                for (int j = 0; j < part1.Count; j++)
                {
                    for (int i = 0; i < part2.Count; i++)
                    {
                        if (part1[j].Age == part2[i].Age && part1[j].Email == part2[i].Email && part1[j].FullName == part2[i].FullName)
                            ans.Add(part1[j]);
                    }
                }
                return ans;
            }
        }
        private static List<Order> andLogicalOrder(List<Order> part1, List<Order> part2)
        {
            if (part1.Count == 0 || part2.Count == 0)//is there are one condition false the and logical is false
                return new List<Order>();
            else
            {    //the two condition is true, return all this two results without duplicate
                List<Order> ans = new List<Order>();
                for (int j = 0; j < part1.Count; j++)
                {
                    for (int i = 0; i < part2.Count; i++)
                    {
                        if (part1[j].Sender == part2[i].Sender && part1[j].Target == part2[i].Target)
                            ans.Add(part1[j]);
                    }
                }
                return ans;
            }
        }

        private static List<User> orLogical(List<User> part1, List<User> part2)
        {
            //logical or is all the values from the two strings without duplicate
            part1.AddRange(part2);
            return part1.Distinct().ToList();
        }
        private static List<Order> orLogicalOrder(List<Order> part1, List<Order> part2)
        {
            //logical or is all the values from the two strings without duplicate
            part1.AddRange(part2);
            return part1.Distinct().ToList();
        }
        #endregion
        #region filters
        public static List<User> getAllUsersBy(Predicate<User> filter)
        {
            List<User> lst = initilizeUserDB();
            return lst.FindAll(filter);
        }
        public static List<Order> getAllOrdersBy(Predicate<Order> filter)
        {
            List<Order> lst = initilizeOrderDB();
            return lst.FindAll(filter);
        }
        private static List<User> getPartUserQuery(string[] filters)
        {
            List<User> users = new List<User>();
            if (filters[0] == "Email")
                users = getAllUsersBy(x => "'" + x.Email + "'" == filters[2]);
            if (filters[0] == "FullName")
                users = getAllUsersBy(x => "'" + x.FullName + "'" == filters[2]);
            if (filters[0] == "Age")
                users = getAllUsersBy(x => Operator(filters[1], x.Age, Convert.ToInt32(filters[2])));
            return users;
        }
        private static List<Data> getPartDataQuery(string[] vs)
        {
            List<User> users = new List<User>();
            List<Order> orders = new List<Order>();
            if (vs[0] == "Email" || vs[0] == "FullName" || vs[0] == "Age")
                users = getPartUserQuery(vs);
            if (vs[0] == "Sender" || vs[0] == "Target")
                orders = getPartOrdersQuery(vs);
            List<Data> data = new List<Data>();
            Data dt = new Data();
            dt.users = users;
            dt.orders = orders;
            data.Add(dt);
            return data;
        }

        private static List<Order> getPartOrdersQuery(string[] filters)
        {
            List<Order> orders = new List<Order>();
            if (filters[0] == "Sender")
                orders = getAllOrdersBy(x => "'" + x.Sender + "'" == filters[2]);
            if (filters[0] == "Target")
                orders = getAllOrdersBy(x => "'" + x.Target + "'" == filters[2]);
            return orders;
        }
        #endregion
        #region convert
        private static List<User> ConvertToUserList(DataTable dt)
        {
            List<User> ans = new List<User>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                User user = new User();
                user.Email = dt.Rows[i]["Email"].ToString();
                user.FullName = dt.Rows[i]["FullName"].ToString();
                user.Age = Convert.ToInt32(dt.Rows[i]["Age"]);
                ans.Add(user);
            }
            return ans;
        }
        private static List<Order> ConvertToOrderList(DataTable dt)
        {
            List<Order> ans = new List<Order>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Order order = new Order();
                order.Sender = dt.Rows[i]["Sender"].ToString();
                order.Target = dt.Rows[i]["Target"].ToString();
                ans.Add(order);
            }
            return ans;
        }
        private static List<Order> ConvertToOrdersList(DataTable dt)
        {
            List<Order> ans = new List<Order>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Order order = new Order();
                order.Sender = dt.Rows[i]["Sender"].ToString();
                order.Target = dt.Rows[i]["Target"].ToString();
                ans.Add(order);
            }
            return ans;
        }
        #endregion

    }
}
