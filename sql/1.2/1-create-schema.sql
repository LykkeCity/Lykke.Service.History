CREATE SCHEMA IF NOT EXISTS history;

CREATE TABLE history.history (
    id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    asset_id text,
    assetpair_id text,
    volume numeric(28,10) NOT NULL,
    type integer NOT NULL,
    create_dt timestamp with time zone NOT NULL,
    context jsonb
);

CREATE TABLE history.orders (
    id uuid NOT NULL,
    matching_id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    type integer NOT NULL,
    side integer NOT NULL,
    status integer NOT NULL,
    assetpair_id text NOT NULL,
    volume numeric(28,10) NOT NULL,
    price numeric(28,10),
    create_dt timestamp with time zone NOT NULL,
    register_dt timestamp with time zone NOT NULL,
    status_dt timestamp with time zone NOT NULL,
    match_dt timestamp with time zone,
    remaining_volume numeric(28,10) NOT NULL,
    reject_reason text,
    lower_limit_price numeric(28,10),
    lower_price numeric(28,10),
    upper_limit_price numeric(28,10),
    upper_price numeric(28,10),
    straight boolean NOT NULL,
    sequence_number bigint NOT NULL
);

ALTER TABLE ONLY history.history
    ADD CONSTRAINT history_pkey PRIMARY KEY (id, wallet_id);

ALTER TABLE ONLY history.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);

CREATE INDEX history_type_pair_date_idx ON history.history USING btree (type, assetpair_id, create_dt DESC);

CREATE INDEX history_wallet_type_date_idx ON history.history USING btree (wallet_id, type, create_dt DESC);

CREATE INDEX orders_wallet_type_date_idx ON history.orders USING btree (wallet_id, type, create_dt DESC);

alter table history.orders alter column volume type numeric;
alter table history.orders alter column price type numeric;
alter table history.orders alter column remaining_volume type numeric;
alter table history.orders alter column lower_limit_price type numeric;
alter table history.orders alter column lower_price type numeric;
alter table history.orders alter column upper_limit_price type numeric;
alter table history.orders alter column upper_price type numeric;
alter table history.history alter column volume type numeric;