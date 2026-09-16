ALTER TABLE T_Calc_Order_Firm_Temp ALTER COLUMN Suffix VARCHAR(25);

TRUNCATE TABLE T_Calc_Order_Firm
DROP INDEX IX_Order_Firm_CustomID ON T_Calc_Order_Firm;
ALTER TABLE T_Calc_Order_Firm DROP COLUMN FirmID;
ALTER TABLE T_Calc_Order_Firm ALTER COLUMN Suffix VARCHAR(25);

ALTER TABLE T_Calc_Order_Firm ADD FirmID AS (
	CONVERT(varchar(50),(((((((('FO'+'-') + 
		upper(Suffix))+'-') + 
		upper(left(Status,(4))))+'-') + 
		replace(Periode,'-',''))+'-') + 'N') + 
		right('0000'+CONVERT(varchar(10),
			case when MonthOffsetLabel='N' then (1) 
			else isnull(TRY_CAST(replace(MonthOffsetLabel,'N+','')
			AS int),(0))+(1) end),(4)))) 
		PERSISTED;

CREATE NONCLUSTERED INDEX IX_Order_Firm_CustomID ON T_Calc_Order_Firm
(
    FirmID ASC
);