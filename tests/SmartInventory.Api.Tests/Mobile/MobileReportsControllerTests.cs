using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Application.Mobile.Reports.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileReportsControllerTests
{
    private readonly Mock<IMobileReportService> _reportServiceMock;
    private readonly MobileReportsController _controller;

    public MobileReportsControllerTests()
    {
        _reportServiceMock = new Mock<IMobileReportService>();
        _controller = new MobileReportsController(_reportServiceMock.Object);
    }

    #region GetRoomFiche

    [Fact]
    public async Task GetRoomFiche_ValidRoom_ReturnsFileWithPdfContentType()
    {
        var roomCode = "A101";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 };

        _reportServiceMock
            .Setup(s => s.GetRoomFicheAsync(roomCode))
            .ReturnsAsync(pdfBytes);

        var result = await _controller.GetRoomFiche(roomCode);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRoomFiche_InvalidRoom_ReturnsNotFound()
    {
        var roomCode = "INVALID";

        _reportServiceMock
            .Setup(s => s.GetRoomFicheAsync(roomCode))
            .ReturnsAsync((byte[]?)null);

        var result = await _controller.GetRoomFiche(roomCode);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetRoomJournal

    [Fact]
    public async Task GetRoomJournal_ValidRoom_ReturnsFileWithPdfContentType()
    {
        var roomCode = "A101";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 };

        _reportServiceMock
            .Setup(s => s.GetRoomJournalAsync(roomCode, null, null))
            .ReturnsAsync(pdfBytes);

        var result = await _controller.GetRoomJournal(roomCode, null, null);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRoomJournal_InvalidRoom_ReturnsNotFound()
    {
        var roomCode = "INVALID";

        _reportServiceMock
            .Setup(s => s.GetRoomJournalAsync(roomCode, null, null))
            .ReturnsAsync((byte[]?)null);

        var result = await _controller.GetRoomJournal(roomCode, null, null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRoomJournal_WithDateRange_PassesParametersToService()
    {
        var roomCode = "A101";
        var from = new DateTime(2025, 1, 1);
        var to = new DateTime(2025, 12, 31);
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _reportServiceMock
            .Setup(s => s.GetRoomJournalAsync(roomCode, from, to))
            .ReturnsAsync(pdfBytes);

        var result = await _controller.GetRoomJournal(roomCode, from, to);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileContents.Should().NotBeEmpty();

        _reportServiceMock.Verify(s => s.GetRoomJournalAsync(roomCode, from, to), Times.Once);
    }

    #endregion

    #region GetDepartmentQr

    [Fact]
    public async Task GetDepartmentQr_ValidDeptId_ReturnsFileWithPngContentType()
    {
        var deptId = Guid.NewGuid();
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };

        _reportServiceMock
            .Setup(s => s.GetDepartmentQrAsync(deptId))
            .ReturnsAsync(pngBytes);

        var result = await _controller.GetDepartmentQr(deptId);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDepartmentQr_InvalidDeptId_ReturnsNotFound()
    {
        var deptId = Guid.NewGuid();

        _reportServiceMock
            .Setup(s => s.GetDepartmentQrAsync(deptId))
            .ReturnsAsync((byte[]?)null);

        var result = await _controller.GetDepartmentQr(deptId);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetDepartmentQrByCode

    [Fact]
    public async Task GetDepartmentQrByCode_ReturnsFileWithPngContentType()
    {
        var code = "CS";
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };

        _reportServiceMock
            .Setup(s => s.GetDepartmentQrByCodeAsync(code))
            .ReturnsAsync(pngBytes);

        var result = await _controller.GetDepartmentQrByCode(code);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    #endregion

    #region GetRoomQr

    [Fact]
    public async Task GetRoomQr_ValidRoomCode_ReturnsFileWithPngContentType()
    {
        var roomCode = "A101";
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };

        _reportServiceMock
            .Setup(s => s.GetRoomQrAsync(roomCode))
            .ReturnsAsync(pngBytes);

        var result = await _controller.GetRoomQr(roomCode);

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRoomQr_InvalidRoomCode_ReturnsNotFound()
    {
        var roomCode = "INVALID";

        _reportServiceMock
            .Setup(s => s.GetRoomQrAsync(roomCode))
            .ReturnsAsync((byte[]?)null);

        var result = await _controller.GetRoomQr(roomCode);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetIsetQr

    [Fact]
    public async Task GetIsetQr_ReturnsFileWithPngContentType()
    {
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };

        _reportServiceMock
            .Setup(s => s.GetIsetQrAsync())
            .ReturnsAsync(pngBytes);

        var result = await _controller.GetIsetQr();

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().NotBeEmpty();
    }

    #endregion
}
