-- 1. Assy-Line 2 Shift
INSERT INTO M_Parameter_Mandatory_Option (LineType, Shift, MandatoryValue, Description, IsDefault, SortOrder) VALUES
('Assy', 2, 82, 'Full Mandatory', 1, 1),
('Assy', 2, 58, 'QCC only OFF', 0, 2),
('Assy', 2, 52, 'AM/PM only OFF', 0, 3),
('Assy', 2, 46, 'QCC & KYM only OFF', 0, 4),
('Assy', 2, 34, 'QCC, I-CARE, KYM OFF', 0, 5),
('Assy', 2, 28, 'QCC, AM/PM OFF', 0, 6),
('Assy', 2, 16, 'QCC, KYM , AM/PM OFF', 0, 7),
('Assy', 2, 4,  'QCC, I-CARE, KYM, & AM/PM OFF', 0, 8);

-- 2. Assy-Line 1 Shift
INSERT INTO M_Parameter_Mandatory_Option (LineType, Shift, MandatoryValue, Description, IsDefault, SortOrder) VALUES
('Assy', 1, 41, 'Full Mandatory', 1, 1),
('Assy', 1, 29, 'QCC only OFF', 0, 2),
('Assy', 1, 26, 'AM/PM only OFF', 0, 3),
('Assy', 1, 23, 'QCC & KYM only OFF', 0, 4),
('Assy', 1, 17, 'QCC, I-CARE, KYM OFF', 0, 5),
('Assy', 1, 14, 'QCC, AM/PM OFF', 0, 6),
('Assy', 1, 8,  'QCC, KYM , AM/PM OFF', 0, 7),
('Assy', 1, 2,  'QCC, I-CARE, KYM, & AM/PM OFF', 0, 8);

-- 3. Machining 2 Shift
INSERT INTO M_Parameter_Mandatory_Option (LineType, Shift, MandatoryValue, Description, IsDefault, SortOrder) VALUES
('Machining', 2, 100, 'Full Mandatory', 1, 1),
('Machining', 2, 76,  'QCC only OFF', 0, 2),
('Machining', 2, 70,  'AM/PM only OFF', 0, 3),
('Machining', 2, 64,  'QCC & KYM only OFF', 0, 4),
('Machining', 2, 52,  'QCC, I-CARE, KYM OFF', 0, 5),
('Machining', 2, 46,  'QCC, AM/PM OFF', 0, 6),
('Machining', 2, 34,  'QCC, KYM , AM/PM OFF', 0, 7),
('Machining', 2, 22,  'QCC, I-CARE, KYM, & AM/PM OFF', 0, 8);

-- 4. Machining 1 Shift
INSERT INTO M_Parameter_Mandatory_Option (LineType, Shift, MandatoryValue, Description, IsDefault, SortOrder) VALUES
('Machining', 1, 50, 'Full Mandatory', 1, 1),
('Machining', 1, 38, 'QCC only OFF', 0, 2),
('Machining', 1, 35, 'AM/PM only OFF', 0, 3),
('Machining', 1, 32, 'QCC & KYM only OFF', 0, 4),
('Machining', 1, 26, 'QCC, I-CARE, KYM OFF', 0, 5),
('Machining', 1, 23, 'QCC, AM/PM OFF', 0, 6),
('Machining', 1, 17, 'QCC, KYM , AM/PM OFF', 0, 7),
('Machining', 1, 11, 'QCC, I-CARE, KYM, & AM/PM OFF', 0, 8);