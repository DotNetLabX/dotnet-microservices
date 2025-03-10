using Blocks.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Orders;
using System.Text.Json;

namespace Ordering.Database.EntityConfigurations.Orders;

public class CustomerEntityConfiguration : EntityConfiguration<Customer>
{
		public override void Configure(EntityTypeBuilder<Customer> entity)
		{
				base.Configure(entity);

				entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

				entity.ComplexProperty(
					e => e.Address, address =>
					{
							address.Property(v => v.Value)
								.HasMaxLength(500).IsRequired();
							address.Property(a => a.City)
								.HasMaxLength(50).IsRequired();
							address.Property(a => a.PostCode).HasMaxLength(50);
					});

				entity.ComplexProperty(
					o => o.PhoneNumber, phoneNumber =>
					{
							phoneNumber.Property(a => a.Number).IsRequired();
							phoneNumber.Property(a => a.CountryCode).IsRequired();
					});

				//JsonSerializerOptions _jsonSettings = new();
				//entity.Property(e => e.Address).HasConversion(
				//		v => JsonSerializer.Serialize(v, _jsonSettings),
				//		v => JsonSerializer.Deserialize<Address>(v, _jsonSettings));

				//entity.Property(e => e.PhoneNumber).HasConversion(
				//		v => JsonSerializer.Serialize(v, _jsonSettings),
				//		v => JsonSerializer.Deserialize<PhoneNumber>(v, _jsonSettings));

		}
}
