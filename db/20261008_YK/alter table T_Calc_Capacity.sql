ALTER TABLE T_Calc_Capacity ADD ProdPlanMonthly_Initial INT;
ALTER TABLE T_Calc_Capacity ADD ProdPlanDaily_Initial INT;
ALTER TABLE T_Calc_Capacity ADD Overtime_DOT_Hours_Initial DECIMAL(10,2);
ALTER TABLE T_Calc_Capacity ADD IdlePerDay_Min_Initial DECIMAL(10,2);
ALTER TABLE T_Calc_Capacity ADD Overtime_Pct_Initial DECIMAL(10,4);