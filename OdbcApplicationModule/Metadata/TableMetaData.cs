using syntez.ServerApp.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class TableMetaData
{
    public string name;        

    public string multicount_name;
    public string singlecount_name;

    public string description;
    public string pk;

    public List<string> references = new List<string>(); //таблицы в которых возможны множественные ссылки на уникальный обьект тек. таблицы
    public Dictionary<string, string> fk = new Dictionary<string, string>();     // ключ- наименование колонки внешнего ключа,  значение - наименование таблицы на которую ссылается ( на первичный ключ которой ссылается внешний)
    public Dictionary<string, ColumnMetaData> columns = new Dictionary<string, ColumnMetaData>();

    public TableMetaData() { }
    public string getTableNameCamelized()
    {
        string name = this.getTableNameCapitalized();

        return name.Substring(0, 1).ToLower() + name.Substring(1, name.Length - 1);
    }
    public string getTableNameCapitalized()
    {
        string capitalized = "";
        foreach(string name in this.name.Split('_'))
        {
            capitalized += name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length-1).ToLower();
        }
        return capitalized;
    }

    public string getPrimaryKey()
    {
        if( this.pk == null)
        {
            foreach( var columnEntry in this.columns )
            {
                if(columnEntry.Value.primary==true)
                {
                    this.pk = columnEntry.Value.name;
                    break;
                }
            }
        }
        return this.pk;            
    }


    public bool ContainsBlob()
    {
             
        foreach (var columnEntry in this.columns)
        {
            if (columnEntry.Value.type.ToLower() == "blob")
            {
                return true;
            }
        }
        return false;
    }
}
