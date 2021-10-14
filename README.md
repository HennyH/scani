# Scani

Simple classroom equipment checkout system.

## Database

ScanCode (Id, QR?, BarCode?, Text?)
User (Id, Role, Username, HashedPassword, ScanCode?)
LoanGroupMembership (UserId, LoanGroupId)
LoanGroup (Id, Name?)
LoanLines (Id, LoanId, UserId, ItemId, Status, ReturnedDate?)
Loan (Id, LoanGroupId, Status, CreatedDate)
Items (Id, Name)
ItemScanCodes (Id, ScanCodeId)
ItemSets (Id, Name)
ItemSetMemberships (EquipmentItemId, EquipmentItemSetId, Name)
Classes (Id, Name?)
ClassItemSets (ClassId, ItemSetId)
ClassStudents (Id, ClassId, UserId)
ClassTeachers (Id, ClassId, UserId)
ClassStudentGroups (Id, ClassId)
ClassStudentGroupMembers (Id, ClassStudentGroupId, UserId)
ClassSchedules (Id, ClassId, DayOfWeek, StartTime, FinishTime)