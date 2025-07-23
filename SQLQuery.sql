select * from users
select * from courses

alter table users add ProfileImage varbinary(max);

CREATE TABLE Courses (
    Id INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Duration INT NOT NULL,
    Instructor NVARCHAR(100) NOT NULL,
    CourseImage VARBINARY(MAX),
);

EXEC sp_help 'sp_InsertCourse';
DELETE FROM Courses;
Delete from users;
DBCC CHECKIDENT ('Courses', RESEED, 0);
DBCC CHECKIDENT ('users', RESEED, 0);



CREATE PROCEDURE sp_InsertCourse
    @Title NVARCHAR(100),
    @Description NVARCHAR(500),
    @Duration INT,
    @Instructor NVARCHAR(100),
    @CourseImage VARBINARY(MAX)
AS
BEGIN
    INSERT INTO Courses (Title, Description, Duration, Instructor, CourseImage)
    VALUES (@Title, @Description, @Duration, @Instructor, @CourseImage);
END


CREATE PROCEDURE sp_UpdateCourse
    @Id INT,
    @Title NVARCHAR(100),
    @Description NVARCHAR(500),
    @Duration INT,
    @Instructor NVARCHAR(100),
    @CourseImage VARBINARY(MAX)
AS
BEGIN
    UPDATE Courses
    SET Title = @Title,
        Description = @Description,
        Duration = @Duration,
        Instructor = @Instructor,
        CourseImage = @CourseImage
    WHERE Id = @Id;
END

