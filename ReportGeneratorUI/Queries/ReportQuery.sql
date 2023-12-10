with RECURSIVE hierarchy as 
(
	SELECT 
		p.Id as 'Id', 
		null as 'UpId', 
		1 as 'Count',
		1 as 'level', 
		cast(Id as text) as 'path' 
		
	from Product as p
	where p.Id not in (SELECT Product from Links)
	
	union all
	
	SELECT 
		l.Product, 
		l.UpProduct, 
		l.Count as 'Count',
		h.level + 1 as 'level', 
		h.path || '.' || cast(l.Product as text) as 'path' 
		from hierarchy as h, Links as l
	where l.UpProduct = h.Id
), 
lpp as 
(
	Select up.Id as 'UpId',
		   p.Id as 'Id',
		   l.Count as 'Count', 
		   p.Price as 'Price'
	from Links as l
	join Product as up on up.Id = l.UpProduct
	join Product as p on p.Id = l.Product
),
cc as
(

	select 
		p.Id,
		coalesce(sum(h2.count), 0) as 'Count',-- of included products per 1 product', 
		coalesce(sum(h2.cost), 0) + p.price as 'Cost',-- of one product'
		p.price as 'Price'
	from Product as p
	left join 
	(
		SELECT 
			lpp.UpId, 
			lpp.Id, 
			lpp.count + coalesce(sum(lpp.count * lpp2.count), 0) as 'count', 
			(lpp.price + sum(coalesce(lpp2.price * lpp2.count, 0))) * lpp.count as 'cost',
			lpp.price
		from lpp
		left join lpp as lpp2 on lpp.Id = lpp2.UpId
		group by lpp.UpId, lpp.Id
		order by lpp.UpId

	) as h2 on h2.UpId = p.Id 
	group by p.Id
)


Select 
	h.Id,
	p.Name,
	h.Count,
	cc.Cost * h.Count as 'Cost',
	p.Price,
	cc.count * h.Count as 'InclusionCount',
	h.level as 'Level'
from
(
	SELECT
		rh.Id, 
		rh.path,
		rh.Level,
		rh.Count
	from hierarchy as rh
) as h

join cc on cc.Id = h.id
join Product as p on p.Id = h.Id
ORDER by h.path


