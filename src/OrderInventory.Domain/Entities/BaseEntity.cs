using System;
using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Domain.Entities;

public abstract class BaseEntity<T>
{
    [Key]
    public T Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }

}

public abstract class BaseEntity : BaseEntity<int>
{

}
