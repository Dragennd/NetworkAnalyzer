CREATE TRIGGER Update_Completed_When_For_Active_Session
AFTER INSERT ON LatencyMonitorReportEntries
FOR EACH ROW
BEGIN
	UPDATE LatencyMonitorReports
	SET CompletedWhen = NEW.TimeStamp
	WHERE ReportID = NEW.ReportID;
END