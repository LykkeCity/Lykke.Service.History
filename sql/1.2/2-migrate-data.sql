insert into history.history
select * from public.history
ON CONFLICT (id, wallet_id) DO NOTHING;

insert into history.orders
select * from public.orders
ON CONFLICT (id) DO NOTHING;