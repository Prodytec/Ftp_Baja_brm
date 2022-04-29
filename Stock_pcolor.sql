create table Ecommstock
(idorden int,Codigo nvarchar(50), cantidad int, procesado int)
go

create procedure [sp_proyectocolor_stock]  
(@idorden as int, @codigo as nvarchar(20), @cantidad as nvarchar(100))
as

insert into Ecommstock(idorden,Codigo, cantidad, procesado) 
values(@idorden, @codigo, @cantidad, 0)


go

--otro store
create procedure [sp_proyectocolor_ecomm_update]
as
begin
update Stock
set Cantidad = cantidad - (select sum(cantidad) from Ecommstock where Codigo = S.IdArticulo and procesado = 0)
from Stock as S
where IdArticulo in (select codigo from Ecommstock where procesado = 0) and iddeposito = '001'
end


update Ecommstock
set procesado = 1
where procesado = 0


