using System;
using System.Collections.Generic;
using System.IO;

namespace Sketchup2GTA.Exporters.Model.RW
{
    /// <summary>
    /// RwSection抽象类
    /// </summary>
    public abstract class RwSection
    {
        private const int HEADER_SIZE = 12;
        
        private uint _sectionType;
        protected RwVersion RwVersion;

        private byte[] _sectionData;

        protected List<RwSection> _childSections = new List<RwSection>();

        /// <summary>
        /// 构造一个RwSection
        /// </summary>
        /// <param name="sectionType"></param>
        /// <param name="rwVersion"></param>
        public RwSection(uint sectionType, RwVersion rwVersion)
        {
            _sectionType = sectionType;
            RwVersion = rwVersion;
        }

        /// <summary>
        /// 准备写入
        /// </summary>
        private void PrepareForWrite()
        {
            _sectionData = CreateSectionData();
            foreach (var childSection in _childSections)
            {
                childSection.PrepareForWrite();
            }
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="bw"></param>
        public void Write(BinaryWriter bw)
        {
            PrepareForWrite();
            WriteSection(bw);
        }

        /// <summary>
        /// 写入Setion 
        /// </summary>
        /// <param name="bw"></param>
        private void WriteSection(BinaryWriter bw)
        {
            WriteHeader(bw);
            bw.Write(_sectionData);
            WriteChildSections(bw);
        }

        protected virtual void WriteSectionData(BinaryWriter bw)
        {
            // By default we do nothing
        }
        /// <summary>
        ///写入头文件 两个uint, 一个int, 都是4个字节，总长12字节
        /// </summary>
        /// <param name="bw"></param>
        private void WriteHeader(BinaryWriter bw)
        {
            bw.Write(_sectionType);
            bw.Write(GetTotalSectionSize());
            bw.Write((int)RwVersion);
        }

        /// <summary>
        /// 写入子Sections
        /// </summary>
        /// <param name="bw"></param>
        private void WriteChildSections(BinaryWriter bw)
        {
            foreach (var childSection in _childSections)
            {
                childSection.WriteSection(bw);
            }
        }

        /// <summary>
        /// 获取自身Section长度，不包括子Sections
        /// </summary>
        /// <returns></returns>
        private uint GetSectionSize()
        {
            return (uint)_sectionData.Length;
        }
        
        /// <summary>
        /// 获取总长度，每个（子Section Size+12）+自身Section Size
        /// </summary>
        /// <returns></returns>
        private uint GetTotalSectionSize()
        {
            uint sectionSize = 0;
            foreach (var childSection in _childSections)
            {
                sectionSize += childSection.GetTotalSectionSize() + HEADER_SIZE;
            }

            return sectionSize + GetSectionSize();
        }

        /// <summary>
        /// 添加一个Section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public RwSection AddSection(RwSection section)
        {
            _childSections.Add(section);
            return this;
        }

        /// <summary>
        /// 添加一个RwStruct Section
        /// </summary>
        protected void AddStructSection()
        {
            AddSection(new RwStruct(CreateStructData(), RwVersion));
        }

        /// <summary>
        /// 创建一个Struct Data 容器
        /// </summary>
        /// <returns></returns>
        private byte[] CreateStructData()
        {
            var memoryStream = new MemoryStream();
            var bw = new BinaryWriter(memoryStream);
            WriteStructSection(bw);
            bw.Flush();
            var data = memoryStream.ToArray();
            bw.Dispose();
            return data;
        }

        /// <summary>
        /// 创建一个Section Data 容器
        /// </summary>
        /// <returns></returns>
        private byte[] CreateSectionData()
        {
            var memoryStream = new MemoryStream();
            var bw = new BinaryWriter(memoryStream);
            WriteSectionData(bw);
            bw.Flush();
            var data = memoryStream.ToArray();
            bw.Dispose();
            return data;
        }

        /// <summary>
        /// 写入Struct Section
        /// </summary>
        /// <param name="bw"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual void WriteStructSection(BinaryWriter bw)
        {
            throw new NotImplementedException("WriteStructSection not implemented");
        }
    }
}