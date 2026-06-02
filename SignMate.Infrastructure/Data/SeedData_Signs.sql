-- ========================================================================
-- FILE SQL TỰ ĐỘNG CHÈN DỮ LIỆU TỌA ĐỘ VÀO DATABASE MỚI (CHƯA CÓ DATA)
-- ========================================================================

SET IDENTITY_INSERT [dbo].[Signs] ON;

-- =====================================
-- Từ vựng số 1: BanTenGi
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    1, 1, 'BanTenGi', '', '', '', 1, '/dataset/BanTenGi.mp4.json'
);

-- =====================================
-- Từ vựng số 2: CamDiec
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    2, 1, 'CamDiec', '', '', '', 2, '/dataset/CamDiec.mp4.json'
);

-- =====================================
-- Từ vựng số 3: CamOn
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    3, 1, 'CamOn', '', '', '', 3, '/dataset/CamOn.mp4.json'
);

-- =====================================
-- Từ vựng số 4: Co
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    4, 1, 'Co', '', '', '', 4, '/dataset/Co.mp4.json'
);

-- =====================================
-- Từ vựng số 5: Gi
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    5, 1, 'Gi', '', '', '', 5, '/dataset/Gi.mp4.json'
);

-- =====================================
-- Từ vựng số 6: KhoeKhong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    6, 1, 'KhoeKhong', '', '', '', 6, '/dataset/KhoeKhong.mp4.json'
);

-- =====================================
-- Từ vựng số 7: Khong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    7, 1, 'Khong', '', '', '', 7, '/dataset/Khong.mp4.json'
);

-- =====================================
-- Từ vựng số 8: Nghe
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    8, 1, 'Nghe', '', '', '', 8, '/dataset/Nghe.mp4.json'
);

-- =====================================
-- Từ vựng số 9: Ten
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    9, 1, 'Ten', '', '', '', 9, '/dataset/Ten.mp4.json'
);

-- =====================================
-- Từ vựng số 10: ToiBiBenh
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    10, 1, 'ToiBiBenh', '', '', '', 10, '/dataset/ToiBiBenh.mp4.json'
);

-- =====================================
-- Từ vựng số 11: ToiBinhThuong
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    11, 1, 'ToiBinhThuong', '', '', '', 11, '/dataset/ToiBinhThuong.mp4.json'
);

-- =====================================
-- Từ vựng số 12: ToiKhoe
-- =====================================
INSERT INTO [dbo].[Signs](
    [Id], [LessonId], [Word], [VideoUrl], [ThumbnailUrl], [Description], [OrderIndex], [ReferenceKeypointData]
) VALUES (
    12, 1, 'ToiKhoe', '', '', '', 12, '/dataset/ToiKhoe.mp4.json'
);

SET IDENTITY_INSERT [dbo].[Signs] OFF;
