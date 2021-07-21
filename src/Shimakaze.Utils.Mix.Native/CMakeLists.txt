cmake_minimum_required(VERSION 3.0.0)
project(Shimakaze.Utils.Mix.Native VERSION 0.1.0)

include(CTest)
enable_testing()

ADD_LIBRARY(Shimakaze.Utils.Mix.Native SHARED Shimakaze.Utils.Mix.Native.cpp)

INSTALL(TARGETS Shimakaze.Utils.Mix.Native RUNTIME DESTINATION ./build/)

SET_TARGET_PROPERTIES(Shimakaze.Utils.Mix.Native PROPERTIES LINKER_LANGUAGE C)

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