"..\..\FFmpeg4\bin\ffmpeg.exe" -i "d:\temp\sps4.h264" -c copy -bsf:v trace_headers -f null -
::"..\..\FFmpeg4\bin\ffmpeg.exe" -i "d:\temp\sps_1280x720_1fps.h264" -c copy -bsf:v trace_headers -f null -
::"..\..\FFmpeg4\bin\ffmpeg.exe" -i "d:\temp\sps_1280x720_1fps.h264" -c copy -bsf:v trace_headers -f null - 2> "output\sps_1280x720_1fps.log"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -i "d:\temp\sps4.h264" -c copy -bsf:v trace_headers -f null - 2> "output\sps4.log"