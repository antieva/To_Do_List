using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ToDoList.Models;
using System;

namespace ToDoList.Models
{
  public class Item
    {
        private string _description;
        private string _dueDate;
        private bool _isDone;
        private int _id;

        // We no longer declare _categoryId here

        public Item(string description, int id = 0, bool done = false)
        {
            _description = description;
            _id = id;
           // categoryId is removed from the constructor
        }

        public override bool Equals(System.Object otherItem)
        {
          if (!(otherItem is Item))
          {
            return false;
          }
          else
          {
             Item newItem = (Item) otherItem;
             bool idEquality = this.GetId() == newItem.GetId();
             bool descriptionEquality = this.GetDescription() == newItem.GetDescription();
             bool dueDateEquality = this.GetDate() == newItem.GetDate();
             // We no longer compare Items' categoryIds in a categoryEquality bool here.
             return (idEquality && descriptionEquality && dueDateEquality);
           }
        }
        public override int GetHashCode()
        {
             return this.GetDescription().GetHashCode();
        }

        public string GetDescription()
        {
            return _description;
        }

        public int GetId()
        {
            return _id;
        }

        public string GetDate()
        {
            return _dueDate;
        }

        public bool GetDone()
        {
            return _isDone;
        }

        public void SetDone(bool maybeDone)
        {
            _isDone = maybeDone;
        }

        public void SetDate(string newDate)
        {
            _dueDate = newDate;
        }

        public void AddCategory(Category newCategory)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO categories_items (category_id, item_id) VALUES (@CategoryId, @ItemId);";

            MySqlParameter category_id = new MySqlParameter();
            category_id.ParameterName = "@CategoryId";
            category_id.Value = newCategory.GetId();
            cmd.Parameters.Add(category_id);

            MySqlParameter item_id = new MySqlParameter();
            item_id.ParameterName = "@ItemId";
            item_id.Value = _id;
            cmd.Parameters.Add(item_id);

            cmd.ExecuteNonQuery();
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public List<Category> GetCategories()
        {
           MySqlConnection conn = DB.Connection();
           conn.Open();
           MySqlCommand cmd = conn.CreateCommand() as MySqlCommand;
           cmd.CommandText = @"SELECT categories.* FROM items
               JOIN categories_items ON (items.id = categories_items.item_id)
               JOIN categories ON (categories_items.category_id = categories.id)
               WHERE items.id = @ItemId;";

           MySqlParameter itemIdParameter = new MySqlParameter();
           itemIdParameter.ParameterName = "@ItemId";
           itemIdParameter.Value = _id;
           cmd.Parameters.Add(itemIdParameter);

           MySqlDataReader rdr = cmd.ExecuteReader() as MySqlDataReader;
           List<Category> categories = new List<Category>{};

           while(rdr.Read())
           {
             int categoryId = rdr.GetInt32(0);
             string categoryName = rdr.GetString(1);
             Category newCategory = new Category(categoryName, categoryId);
             categories.Add(newCategory);
           }
           conn.Close();
           if (conn != null)
           {
               conn.Dispose();
           }
           return categories;
       }

        // We've removed the GetCategoryId() method entirely.

        public void Save()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO items (description, due_date) VALUES (@description, @dueDate);";

            MySqlParameter description = new MySqlParameter();
            description.ParameterName = "@description";
            description.Value = this._description;
            cmd.Parameters.Add(description);

            MySqlParameter dueDate = new MySqlParameter();
            dueDate.ParameterName = "@dueDate";
            dueDate.Value = this._dueDate;
            cmd.Parameters.Add(dueDate);

            // Code to declare, set, and add values to a categoryId SQL parameters has also been removed.

            cmd.ExecuteNonQuery();
            _id = (int) cmd.LastInsertedId;
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public static List<Item> GetAll()
        {
            List<Item> allItems = new List<Item> {};
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM items ORDER BY due_date ASC;";
            var rdr = cmd.ExecuteReader() as MySqlDataReader;
            while(rdr.Read())
            {
              int itemId = rdr.GetInt32(0);
              string itemDueDate = rdr.GetString(2);
              string itemDescription = rdr.GetString(1);
              // We no longer need to read categoryIds from our items table here.
              // Constructor below no longer includes a itemCategoryId parameter:
              Item newItem = new Item(itemDescription, itemId);
              newItem.SetDate(itemDueDate);
              allItems.Add(newItem);
            }
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
            return allItems;
        }

        public static Item Find(int id)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM items WHERE id = (@searchId);";

            MySqlParameter searchId = new MySqlParameter();
            searchId.ParameterName = "@searchId";
            searchId.Value = id;
            cmd.Parameters.Add(searchId);

            var rdr = cmd.ExecuteReader() as MySqlDataReader;
            int itemId = 0;
            string itemName = "";
            string itemDueDate = "";
            // We remove the line setting a itemCategoryId value here.

            while(rdr.Read())
            {
              itemId = rdr.GetInt32(0);
              itemName = rdr.GetString(1);
              itemDueDate = rdr.GetString(2);
              // We no longer read the itemCategoryId here, either.
            }

            // Constructor below no longer includes a itemCategoryId parameter:
            Item newItem = new Item(itemName, itemId);
            newItem.SetDate(itemDueDate);
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

            return newItem;
        }

        public void UpdateDescription(string newDescription)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"UPDATE items SET description = @newDescription WHERE id = @searchId;";

            MySqlParameter searchId = new MySqlParameter();
            searchId.ParameterName = "@searchId";
            searchId.Value = _id;
            cmd.Parameters.Add(searchId);

            MySqlParameter description = new MySqlParameter();
            description.ParameterName = "@newDescription";
            description.Value = newDescription;
            cmd.Parameters.Add(description);

            cmd.ExecuteNonQuery();
            _description = newDescription;
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

        }

        public void Delete()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM items WHERE id = @ItemId; DELETE FROM categories_items WHERE item_id = @ItemId;";

            MySqlParameter itemIdParameter = new MySqlParameter();
            itemIdParameter.ParameterName = "@ItemId";
            itemIdParameter.Value = this.GetId();
            cmd.Parameters.Add(itemIdParameter);

            cmd.ExecuteNonQuery();
            if (conn != null)
            {
                conn.Close();
            }
        }

        public static void DeleteAll()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM items;";
            cmd.ExecuteNonQuery();
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }
    }
}
