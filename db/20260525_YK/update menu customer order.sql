INSERT INTO M_MenuGroup VALUES(12, 'Monthly Calculation', 12)

UPDATE M_Menu
SET MenuName = 'CustomerOrder',
	MenuDescription = 'Customer Order',
	MenuGroupID = 12
WHERE MenuID = 111

INSERT INTO M_MenuGroup VALUES(13, 'Daily Calculation', 13)
INSERT INTO M_Menu VALUES(121, 'OrderTmmin', 'Order TMMIN', 121, 13)