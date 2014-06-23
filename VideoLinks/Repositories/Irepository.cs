using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoLinks.Repositories
{

    /// <summary>
    /// Interface for basic Repository actions
    /// </summary>
    /// <typeparam name="T">The type of Repository</typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Specifies methods/properties that should be implemented by ALL repositories
        /// </summary>
        IQueryable<T> Items { get; }
        T AddItem(T newItem);
        T UpdateItem(T newItem);
        List<T> ItemList();
        T FindByID(int id);
        T RemoveByID(int id);
        T Remove(T toBeRemoved);
        int SaveChanges();

    }

}