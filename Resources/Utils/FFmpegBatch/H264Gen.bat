

"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=30:size=1280x720:rate=30 -vcodec h264 -bf 0 -pix_fmt yuv444p "output\testsrc_1280x720_yuv444p_30fps_30sec_bf0.h264"

::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=30:size=1280x720:rate=30 -vcodec h264 -bf 0 -pix_fmt nv12 "output\testsrc_1280x720_nv12_30fps_30sec_bf0.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=30:size=1280x720:rate=30 -vcodec h264 -pix_fmt yuv420p "output\testsrc_1280x720_yuv420p_30fps_30sec.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=30:size=1280x720:rate=30 -vcodec h264 -bf 0 -pix_fmt yuv420p "output\testsrc_1280x720_yuv420p_30fps_10sec_bf0.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i smptebars=duration=30:size=1280x720:rate=30 -vcodec h264 -bf 0 -pix_fmt nv12 "output\smptebars_1280x720_nv12_30fps_30sec_bf0.h264"