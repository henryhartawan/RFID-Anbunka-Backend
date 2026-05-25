INSERT INTO M_MenuGroup (MenuGroupID, MenuGroupName, MenuGroupSeq)
	VALUES (11, 'Capacity Calculation', 11)

INSERT INTO M_Menu (MenuID, MenuName, MenuDescription, MenuSeq, MenuGroupID)
VALUES (116, 'FirmOrder', 'Firm Tentatif Order', 116, 11)

INSERT INTO M_Menu (MenuID, MenuName, MenuDescription, MenuSeq, MenuGroupID)
VALUES (117, 'SummaryOrder', 'Summary Order', 117, 11)