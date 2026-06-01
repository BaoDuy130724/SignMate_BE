-- ========================================================================
-- FILE SQL TỰ ĐỘNG CHÈN DỮ LIỆU TỌA ĐỘ VÀO DATABASE MỚI (CHƯA CÓ DATA)
-- Hướng dẫn:
-- 1. Thay thế '"ae9d36ab-8606-4694-bced-a3c58a4427e6"' bằng ID của Bài Học thực tế.
-- 2. Nếu cột Id tự tăng (Serial), hãy xóa `NEWID(),` và cột `[Id],` đi.
-- 3. Sửa lại tên tiếng Việt có dấu ở cột 'Word' cho đẹp nhé!
-- ========================================================================

-- =====================================
-- Từ vựng số 1: BanTenGi
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'BanTenGi', '', '', '', 1, '/dataset/BanTenGi.mp4.json'
);

-- =====================================
-- Từ vựng số 2: CamDiec
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'CamDiec', '', '', '', 2, '/dataset/CamDiec.mp4.json'
);

-- =====================================
-- Từ vựng số 3: CamOn
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'CamOn', '', '', '', 3, '/dataset/CamOn.mp4.json'
);

-- =====================================
-- Từ vựng số 4: Co
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'Co', '', '', '', 4, '/dataset/Co.mp4.json'
);

-- =====================================
-- Từ vựng số 5: Gi
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'Gi', '', '', '', 5, '/dataset/Gi.mp4.json'
);

-- =====================================
-- Từ vựng số 6: KhoeKhong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'KhoeKhong', '', '', '', 6, '/dataset/KhoeKhong.mp4.json'
);

-- =====================================
-- Từ vựng số 7: Khong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'Khong', '', '', '', 7, '/dataset/Khong.mp4.json'
);

-- =====================================
-- Từ vựng số 8: Nghe
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'Nghe', '', '', '', 8, '/dataset/Nghe.mp4.json'
);

-- =====================================
-- Từ vựng số 9: Ten
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'Ten', '', '', '', 9, '/dataset/Ten.mp4.json'
);

-- =====================================
-- Từ vựng số 10: ToiBiBenh
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'ToiBiBenh', '', '', '', 10, '/dataset/ToiBiBenh.mp4.json'
);

-- =====================================
-- Từ vựng số 11: ToiBinhThuong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'ToiBinhThuong', '', '', '', 11, '/dataset/ToiBinhThuong.mp4.json'
);

-- =====================================
-- Từ vựng số 12: ToiKhoe
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    NEWID(), 'B376520F-2A6C-4726-A593-9EEFFDD2E095', 'ToiKhoe', '', '', '', 12, '/dataset/ToiKhoe.mp4.json'
);
