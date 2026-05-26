
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE sp_Upload_SKID
	@EntryUser		nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Err varchar(50)

	BEGIN TRY
		IF EXISTS(
			SELECT	a.A1
			FROM	DataExcel a
					LEFT JOIN T_DN c on LEFT(a.A1,16) = c.DN_No
			WHERE	a.Entry_User = @EntryUser
					and a.A1 <> 'Kanban No' and a.A1 <> ''
					and ISNULL(c.DN_No,'') = ''
		)
		BEGIN
			RAISERROR('DN No not exists', 16, 1)
		END
		
		IF EXISTS(
			SELECT	a.A1
			FROM	DataExcel a
					LEFT JOIN T_DN c on LEFT(a.A1,16) = c.DN_No
			WHERE	a.Entry_User = @EntryUser
					and a.A1 <> 'Kanban No' and a.A1 <> ''
					and ISNULL(c.DN_No,'') <> '' and ISNULL(c.PI_No,'') = '' and ISNULL(DN_Status,1) = 1
		)
		BEGIN
			RAISERROR('DN not yet print packing instruction', 16, 1)
		END
		
		IF EXISTS(
			SELECT	a.A1
			FROM	DataExcel a
					LEFT JOIN M_Part_Order c on SUBSTRING(a.A1, 17, 14) = c.PartNumber
			WHERE	a.Entry_User = @EntryUser
					and a.A1 <> 'Kanban No' and a.A1 <> ''
					and ISNULL(c.PartNumber,'') = ''
		)
		BEGIN
			RAISERROR('Part Number not exists in Master Part Order', 16, 1)
		END
		
		IF EXISTS(
			SELECT	1
			FROM	DataExcel a
					LEFT JOIN (
						SELECT	DN_No, Part_No, SUM(Total_Kanban) as total
						FROM	T_DN
						WHERE	ISNULL(DN_Status,1) = 1
						GROUP BY DN_No, Part_No
					) b on LEFT(a.A1,16) = b.DN_No and SUBSTRING(a.A1, 17, 14) = b.Part_No
			WHERE	a.Entry_User = @EntryUser
					and a.A1 <> 'Kanban No' and a.A1 <> ''
					and b.total < CAST(RIGHT(a.A1,3) as int)
		)
		BEGIN
			RAISERROR('Total box scan exceed total box DN', 16, 1)
		END
		
		IF EXISTS(
			SELECT	a.A1
			FROM	DataExcel a
					LEFT JOIN T_SKID c on a.A1 = c.KanbanNo
			WHERE	a.Entry_User = @EntryUser
					and a.A1 <> 'Kanban No' and a.A1 <> ''
					and ISNULL(c.SKID_ID,'') <> ''
		)
		BEGIN
			RAISERROR('Box Label already scanned', 16, 1)
		END

		;WITH CTE_EXCEL AS
		(
			SELECT	A1 AS KanbanNo,
					LEFT(A1, 16) AS DN_No,
					SUBSTRING(A1, 17, 14) AS Part_No,
					RIGHT(A1, 3) AS KanbanSeq
			FROM	DataExcel
			WHERE	Entry_User = @EntryUser
					and A1 <> 'Kanban No' and A1 <> ''
		),
		CTE_JOIN AS
		(
			SELECT	E.KanbanNo,
					E.KanbanSeq,
					D.PI_No,
					D.DN_No,
					D.Part_No,
					D.ExCore,
					D.Job_No,
					D.Supplier_Code
			FROM	CTE_EXCEL E
					INNER JOIN (
						SELECT	DISTINCT DN_No,
								PI_No,
								Part_No,
								ExCore,
								Job_No,
								Supplier_Code
						FROM	T_DN
						WHERE	ISNULL(DN_Status,1) = 1 and ISNULL(PI_No,'') <> ''
					) D ON  E.DN_No = D.DN_No AND E.Part_No = D.Part_No
		),
		CTE_MAX_SKID AS
		(
			SELECT	MAX(CAST(RIGHT(SKID_ID, 5) AS INT)) AS MaxSeq
			FROM	T_SKID
			WHERE	LEFT(SKID_ID, 6) = FORMAT(GETDATE(), 'yyMMdd')
		),
		CTE_PI AS
		(
			SELECT	DISTINCT PI_No,
					Supplier_Code,
					DENSE_RANK() OVER (ORDER BY PI_No) AS NewSeq
			FROM	CTE_JOIN
		),
		CTE_FINAL AS
		(
			SELECT	SKID_ID = FORMAT(GETDATE(), 'yyMMdd') + J.Supplier_Code + RIGHT('00000' + CAST(ISNULL(M.MaxSeq, 0) + P.NewSeq AS VARCHAR),5),
					J.KanbanNo,
					J.PI_No,
					J.DN_No,
					J.Part_No,
					J.ExCore,
					J.Job_No,
					J.KanbanSeq
			FROM	CTE_JOIN J
					INNER JOIN CTE_PI P ON J.PI_No = P.PI_No
			CROSS JOIN CTE_MAX_SKID M
		)

		INSERT INTO T_SKID (
			SKID_ID,
			KanbanNo,
			PI_No,
			DN_No,
			Part_No,
			ExCore,
			Job_No,
			KanbanSeq,
			ScanBy,
			ScanDate
		)
		SELECT	F.SKID_ID,
				F.KanbanNo,
				F.PI_No,
				F.DN_No,
				F.Part_No,
				F.ExCore,
				F.Job_No,
				F.KanbanSeq,
				@EntryUser,
				GETDATE()
		FROM	CTE_FINAL F
		WHERE	NOT EXISTS
				(
					SELECT 1
					FROM T_SKID T
					WHERE T.KanbanNo = F.KanbanNo
					  AND T.ExCore   = F.ExCore
				);

		SELECT Remarks = ''
	END TRY

	BEGIN CATCH
		SELECT Remarks = ERROR_MESSAGE()
	END CATCH
END
GO
