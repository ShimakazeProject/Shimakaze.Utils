cmake_minimum_required(VERSION 3.0.0)
project(Shimakaze.Utils.Native.Mix VERSION 0.1.0)

include(CTest)
enable_testing()

ADD_LIBRARY(Shimakaze.Utils.Native.Mix SHARED Shimakaze.Utils.Native.Mix.cpp)

INSTALL(TARGETS Shimakaze.Utils.Native.Mix RUNTIME DESTINATION ./build/)

SET_TARGET_PROPERTIES(Shimakaze.Utils.Native.Mix PROPERTIES LINKER_LANGUAGE C)

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)

if (CMAKE_HOST_WIN32)
    set(WINDOWS 1)
elseif(CMAKE_HOST_APPLE)
    set(MACOS 1)
elseif(CMAKE_HOST_UNIX)
    set(LINUX 1)
endif()

if(WINDOWS)
    link_directories("${PROJECT_SOURCE_DIR}/platforms/win") 
elseif(LINUX)
    link_directories("${PROJECT_SOURCE_DIR}/platforms/linux") 
elseif(MACOS)
    link_directories("${PROJECT_SOURCE_DIR}/platforms/macos") 
endif()