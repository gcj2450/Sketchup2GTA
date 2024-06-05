using System;
using System.Collections.Generic;
using System.IO;

namespace Sketchup2GTA.Exporters.Model.RW
{
    /// <summary>
    /// RwSection������
    /// </summary>
    public abstract class RwSection
    {
        private const int HEADER_SIZE = 12;
        
        private uint _sectionType;
        protected RwVersion RwVersion;

        private byte[] _sectionData;

        protected List<RwSection> _childSections = new List<RwSection>();

        /// <summary>
        /// ����һ��RwSection
        /// </summary>
        /// <param name="sectionType"></param>
        /// <param name="rwVersion"></param>
        public RwSection(uint sectionType, RwVersion rwVersion)
        {
            _sectionType = sectionType;
            RwVersion = rwVersion;
        }

        /// <summary>
        /// ׼��д��
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
        /// д��
        /// </summary>
        /// <param name="bw"></param>
        public void Write(BinaryWriter bw)
        {
            PrepareForWrite();
            WriteSection(bw);
        }

        /// <summary>
        /// д��Setion 
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
        ///д��ͷ�ļ� ����uint, һ��int, ����4���ֽڣ��ܳ�12�ֽ�
        /// </summary>
        /// <param name="bw"></param>
        private void WriteHeader(BinaryWriter bw)
        {
            bw.Write(_sectionType);
            bw.Write(GetTotalSectionSize());
            bw.Write((int)RwVersion);
        }

        /// <summary>
        /// д����Sections
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
        /// ��ȡ����Section���ȣ���������Sections
        /// </summary>
        /// <returns></returns>
        private uint GetSectionSize()
        {
            return (uint)_sectionData.Length;
        }
        
        /// <summary>
        /// ��ȡ�ܳ��ȣ�ÿ������Section Size+12��+����Section Size
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
        /// ���һ��Section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public RwSection AddSection(RwSection section)
        {
            _childSections.Add(section);
            return this;
        }

        /// <summary>
        /// ���һ��RwStruct Section
        /// </summary>
        protected void AddStructSection()
        {
            AddSection(new RwStruct(CreateStructData(), RwVersion));
        }

        /// <summary>
        /// ����һ��Struct Data ����
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
        /// ����һ��Section Data ����
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
        /// д��Struct Section
        /// </summary>
        /// <param name="bw"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual void WriteStructSection(BinaryWriter bw)
        {
            throw new NotImplementedException("WriteStructSection not implemented");
        }
    }
}