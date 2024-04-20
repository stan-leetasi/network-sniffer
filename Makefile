EXEORIG=sniffer
EXE=ipk-sniffer

SOURCE=sniffer/sniffer.csproj
OUTPUTDIR=sniffer/bin/Debug/net8.0/
TARGETDIR=$(CURDIR)

PDB=sniffer.pdb
DL=sniffer.dll
RUNTC=sniffer.runtimeconfig.json
DEPS=sniffer.deps.json
SPCAP=SharpPcap.dll
PACKDNT=PacketDotNet.dll

all: $(EXE)

$(EXE):
	dotnet build $(SOURCE)
	mv $(OUTPUTDIR)$(EXEORIG) $(TARGETDIR)/$(EXE)
	mv $(OUTPUTDIR)$(PDB) $(TARGETDIR)
	mv $(OUTPUTDIR)$(RUNTC) $(TARGETDIR)
	mv $(OUTPUTDIR)$(DL) $(TARGETDIR)
	mv $(OUTPUTDIR)$(DEPS) $(TARGETDIR)
	mv $(OUTPUTDIR)$(SPCAP) $(TARGETDIR)
	mv $(OUTPUTDIR)$(PACKDNT) $(TARGETDIR)

run:
	./ipk-sniffer

clean:
	rm $(EXE) $(PDB) $(RUNTC) $(DL) $(DEPS) $(SPCAP) $(PACKDNT)
	rm -r sniffer/bin
	rm -r sniffer/obj

# Phony targets
.PHONY: all clean
